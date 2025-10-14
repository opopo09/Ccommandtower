using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ★ 追加: AudioSourceコンポーネントを必須にする
[RequireComponent(typeof(AudioSource))]
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
    public Vector3 attackCenterOffset = Vector3.zero;

    // ★ 追加: SE設定用のヘッダーと変数
    [Header("SE設定")]
    public AudioClip attackSound;

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

    [Header("ターゲット設定（優先度）")]
    public string[] priorityTargetTagsLv1;
    public string[] priorityTargetTagsLv2;
    public string[] priorityTargetTagsLv3;

    [Header("回避設定")]
    public float avoidanceRadius = 2f;
    public float avoidanceForce = 5f;
    [Tooltip("回避時の最大速度の倍率。1にするとmoveSpeedを超えません。")]
    public float maxAvoidanceSpeedMultiplier = 1.5f;

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

    private Animator animator;
    private AudioSource audioSource; // ★ 追加: AudioSourceを格納するための変数

    void Start()
    {
        currentHP = maxHP;
        hpBar?.SetHP(currentHP, maxHP);
        spriteRenderer = GetComponent<SpriteRenderer>();
        wanderTarget = transform.position;

        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>(); // ★ 追加: このオブジェクトのAudioSourceコンポーネントを取得

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
        FindNearestEnemy();
        if (nearestEnemy == null)
        {
            Wander();
        }
        else
        {
            ChaseAndAttack();
        }
        UpdateFadeAlpha();
    }

    Vector3 CalculateAvoidanceVector()
    {
        Vector3 avoidanceVector = Vector3.zero;
        int objectsFound = 0;
        foreach (AvoidanceTarget target in AvoidanceTarget.AllTargets)
        {
            if (target == null || target.gameObject == this.gameObject) continue;
            float dist = Vector3.Distance(this.transform.position, target.transform.position);
            if (dist < avoidanceRadius)
            {
                Vector3 directionAway = (transform.position - target.transform.position);
                if (directionAway.magnitude > 0)
                {
                    avoidanceVector += directionAway.normalized / dist;
                    objectsFound++;
                }
            }
        }
        if (objectsFound > 0)
        {
            avoidanceVector /= objectsFound;
        }
        return avoidanceVector;
    }

    void FindNearestEnemy()
    {
        Vector3 centerPos = transform.position + attackCenterOffset;
        nearestEnemy = null;
        float minDistance = float.MaxValue;

        System.Action<string[]> findAction = (tags) =>
        {
            if (tags == null) return;
            foreach (string tag in tags)
            {
                if (string.IsNullOrEmpty(tag)) continue;
                try
                {
                    GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
                    foreach (GameObject target in targets)
                    {
                        float dist = Vector3.Distance(centerPos, target.transform.position);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            nearestEnemy = target;
                        }
                    }
                }
                catch (UnityException) { continue; }
            }
        };

        findAction(priorityTargetTagsLv1);
        findAction(priorityTargetTagsLv2);
        findAction(priorityTargetTagsLv3);
    }

    void ChaseAndAttack()
    {
        if (nearestEnemy == null) return;
        Vector3 centerPos = transform.position + attackCenterOffset;
        float dist = Vector3.Distance(centerPos, nearestEnemy.transform.position);
        if (dist > attackRange)
        {
            Vector3 dir = (nearestEnemy.transform.position - centerPos).normalized;
            Vector3 desiredVelocity = dir * moveSpeed;
            Vector3 avoidanceVector = CalculateAvoidanceVector();
            currentVelocity = desiredVelocity + avoidanceVector * avoidanceForce;
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

        // ★ 追加: SEを再生する処理
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

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
                SetNewWanderTarget();
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
            SetNewWanderTarget();
        }
        Vector3 avoidanceVector = CalculateAvoidanceVector();
        Vector3 finalVelocity = currentVelocity + avoidanceVector * avoidanceForce;
        MoveAndFace(finalVelocity);
        if (Vector3.Distance(transform.position, wanderTarget) < 0.2f)
        {
            currentVelocity = Vector3.zero;
            isStopped = true;
            stopTimer = Random.Range(stopDurationMin, stopDurationMax);
        }
    }

    void SetNewWanderTarget()
    {
        Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(1f, wanderRadius);
        wanderTarget = transform.position + new Vector3(circle.x, circle.y, 0);
        currentVelocity = (wanderTarget - transform.position).normalized * moveSpeed;
        wanderTimer = Random.Range(wanderIntervalMin, wanderIntervalMax);
    }

    void MoveAndFace(Vector3 velocity)
    {
        if (velocity == Vector3.zero) return;
        if (allowFlip && spriteRenderer != null)
        {
            spriteRenderer.flipX = velocity.x < 0;
        }
        float maxSpeed = moveSpeed * maxAvoidanceSpeedMultiplier;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * moveSpeedMultiplier * Time.deltaTime;
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

    public void Heal(float amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;
        hpBar?.SetHP(currentHP, maxHP);
    }

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

    void StartFadeAlpha()
    {
        isFading = true;
        alphaRestoreTimer = fadeDuration;
        SetAlpha(fadeAlpha);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + attackCenterOffset, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
    }
}