using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer))]
public class Ally : MonoBehaviour
{
    [Header("体力設定")]
    public float maxHP = 100f;
    public AllyHPBar hpBar;

    [Header("攻撃設定")]
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public float moveSpeed = 3f;

    [Header("ウロチョロ設定")]
    public float wanderRadius = 5f;
    public float wanderIntervalMin = 2f;
    public float wanderIntervalMax = 5f;
    [Range(0f, 1f)] public float stopProbability = 0.3f;
    public float stopDurationMin = 1f;
    public float stopDurationMax = 3f;

    [Header("透明度設定")]
    public float fadeAlpha = 0.3f;
    public float fadeDuration = 0.5f;

    [Header("ターゲット設定")]
    public string[] priorityTargetTags;
    public string[] normalTargetTags;

    [Header("スローエフェクト")]
    public GameObject slowEffectPrefab;
    public Vector3 slowEffectOffset = Vector3.zero;

    [Header("反転設定")]
    public GameObject flipPrefab;

    private float currentHP;
    private float lastAttackTime = -999f;

    private GameObject nearestEnemy;
    private Vector3 wanderTarget;
    private float wanderTimer = 0f;
    private float stopTimer = 0f;
    private bool isStopped = false;
    private Vector3 currentVelocity = Vector3.zero;

    private SpriteRenderer spriteRenderer;
    private float alphaRestoreTimer = 0f;
    private bool isFading = false;

    private float moveSpeedMultiplier = 1f;

    private bool isSlowed = false;
    private GameObject slowEffectInstance;

    private bool allowFlip = false;

    public void SetMoveSpeedMultiplier(float multiplier)
    {
        moveSpeedMultiplier = multiplier;

        bool nowSlowed = multiplier < 1f;

        if (nowSlowed && !isSlowed)
            StartSlowEffect();
        else if (!nowSlowed && isSlowed)
            StopSlowEffect();

        isSlowed = nowSlowed;
    }

    void StartSlowEffect()
    {
        if (slowEffectPrefab != null && slowEffectInstance == null)
        {
            slowEffectInstance = Instantiate(slowEffectPrefab, transform);
            slowEffectInstance.transform.localPosition = slowEffectOffset;
        }
    }

    void StopSlowEffect()
    {
        if (slowEffectInstance != null)
        {
            Destroy(slowEffectInstance);
            slowEffectInstance = null;
        }
    }

    void Start()
    {
        currentHP = maxHP;
        hpBar?.SetHP(currentHP, maxHP);

        spriteRenderer = GetComponent<SpriteRenderer>();
        wanderTarget = transform.position;
        wanderTimer = 0f;

        // flipPrefabとの一致判定（Editor or Name比較）
        if (flipPrefab == null)
        {
            allowFlip = true;
        }
#if UNITY_EDITOR
        else
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            allowFlip = prefab == flipPrefab;
        }
#else
        else
        {
            allowFlip = gameObject.name.Contains(flipPrefab.name);
        }
#endif
    }

    void Update()
    {
        if (nearestEnemy == null)
            FindNearestEnemy();

        if (nearestEnemy == null)
            Wander();
        else
            ChaseAndAttack();

        UpdateFadeAlpha();
    }

    void FindNearestEnemy()
    {
        float closestDist = Mathf.Infinity;
        nearestEnemy = null;

        foreach (string tag in priorityTargetTags)
        {
            foreach (GameObject t in GameObject.FindGameObjectsWithTag(tag))
            {
                float dist = Vector3.Distance(transform.position, t.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    nearestEnemy = t;
                }
            }
        }

        if (nearestEnemy == null)
        {
            foreach (string tag in normalTargetTags)
            {
                foreach (GameObject t in GameObject.FindGameObjectsWithTag(tag))
                {
                    float dist = Vector3.Distance(transform.position, t.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        nearestEnemy = t;
                    }
                }
            }
        }
    }

    void ChaseAndAttack()
    {
        if (nearestEnemy == null) return;

        float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);

        if (dist > attackRange)
        {
            Vector3 dir = (nearestEnemy.transform.position - transform.position).normalized;
            currentVelocity = dir * moveSpeed;
            isStopped = false;
            MoveAndFace(currentVelocity);
        }
        else
        {
            currentVelocity = Vector3.zero;
            isStopped = true;
            TryAttack();
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (nearestEnemy == null) return;

        var enemy = nearestEnemy.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
            return;
        }

        var boss = nearestEnemy.GetComponent<DragonBoss>();
        if (boss != null)
        {
            boss.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
        }
    }

    void Wander()
    {
        if (isStopped)
        {
            stopTimer -= Time.deltaTime;
            if (stopTimer <= 0f)
            {
                isStopped = false;
                Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(1f, wanderRadius);
                wanderTarget = transform.position + new Vector3(circle.x, circle.y, 0);
                currentVelocity = (wanderTarget - transform.position).normalized * moveSpeed;
                wanderTimer = Random.Range(wanderIntervalMin, wanderIntervalMax);
            }
            return;
        }

        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f)
        {
            if (Random.value < stopProbability)
            {
                isStopped = true;
                stopTimer = Random.Range(stopDurationMin, stopDurationMax);
                currentVelocity = Vector3.zero;
                return;
            }

            Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(1f, wanderRadius);
            wanderTarget = transform.position + new Vector3(circle.x, circle.y, 0);
            currentVelocity = (wanderTarget - transform.position).normalized * moveSpeed;
            wanderTimer = Random.Range(wanderIntervalMin, wanderIntervalMax);
        }

        MoveAndFace(currentVelocity);

        if (Vector3.Distance(transform.position, wanderTarget) < 0.2f)
        {
            currentVelocity = Vector3.zero;
            isStopped = true;
            stopTimer = Random.Range(stopDurationMin, stopDurationMax);
        }
    }

    void MoveAndFace(Vector3 velocity)
    {
        if (velocity == Vector3.zero) return;

        if (allowFlip && spriteRenderer != null)
        {
            spriteRenderer.flipX = velocity.x < 0;
        }

        transform.position += velocity * moveSpeedMultiplier * Time.deltaTime;
    }

    void StartFadeAlpha()
    {
        isFading = true;
        alphaRestoreTimer = fadeDuration;
        SetAlpha(fadeAlpha);
    }

    void UpdateFadeAlpha()
    {
        if (!isFading) return;

        alphaRestoreTimer -= Time.deltaTime;
        if (alphaRestoreTimer <= 0f)
        {
            SetAlpha(1f);
            isFading = false;
        }
        else
        {
            float t = alphaRestoreTimer / fadeDuration;
            float a = Mathf.Lerp(1f, fadeAlpha, t);
            SetAlpha(a);
        }
    }

    void SetAlpha(float alpha)
    {
        if (spriteRenderer == null) return;
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        hpBar?.SetHP(currentHP, maxHP);

        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
