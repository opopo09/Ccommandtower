using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DragonBoss : MonoBehaviour
{
    [Header("ステータス")]
    public float bossMaxHP = 300f;
    public float moveSpeed = 2f;
    public float stopApproachDistance = 1.5f;

    [Header("範囲攻撃")]
    public float breathDamagePerSecond = 10f;
    public float breathDuration = 2f;
    public float breathRange = 5f;
    [Range(0f, 180f)] public float breathAngle = 45f;
    public float attackCooldown = 5f;

    [Header("視覚効果")]
    public GameObject breathEffectPrefab;
    public Transform breathEffectSpawnPoint;

    [Header("HPバー")]
    public BossHPBar bossHPBar;

    [Header("透明度設定")]
    public float fadeAlpha = 0.3f;
    public float fadeDuration = 0.5f;

    [Header("ターゲットタグ（優先度高→低）")]
    public string[] highPriorityTags = new string[] { "AllyBoss" };
    public string[] midPriorityTags = new string[] { "AllyHealer" };
    public string[] lowPriorityTags = new string[] { "Ally" };
    public string[] avoidTargetTags = new string[] { "Ally" };

    private float currentHP;
    private float lastAttackTime = -999f;
    private float breathTimer;
    private bool isAttacking;

    private GameObject currentBreathEffect;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

    private float alphaRestoreTimer;
    private bool isFading;

    void Start()
    {
        currentHP = bossMaxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        if (bossHPBar != null)
            bossHPBar.SetHP(currentHP, bossMaxHP);
    }

    void Update()
    {
        GameObject target = FindClosestTarget(highPriorityTags);
        if (target == null)
            target = FindClosestTarget(midPriorityTags);
        if (target == null)
            target = FindClosestTarget(lowPriorityTags);

        GameObject avoid = FindClosestTarget(avoidTargetTags);

        if (isAttacking)
        {
            breathTimer -= Time.deltaTime;
            ApplyBreathDamage(Time.deltaTime);

            if (breathTimer <= 0f)
            {
                EndBreathAttack();
                lastAttackTime = Time.time;
            }

            UpdateFadeAlpha();
            return;
        }

        if (target != null)
        {
            AdjustFacingDirection(target.transform.position);
            float dist = Vector2.Distance(transform.position, target.transform.position);

            if (dist <= breathRange)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                    StartBreathAttack();
            }
            else
            {
                MoveTowards(target.transform.position);
            }
        }
        else if (avoid != null)
        {
            float dist = Vector2.Distance(transform.position, avoid.transform.position);
            if (dist < stopApproachDistance)
            {
                Vector3 dir = (transform.position - avoid.transform.position).normalized;
                transform.position += dir * moveSpeed * Time.deltaTime;
            }
        }

        UpdateFadeAlpha();
    }

    GameObject FindClosestTarget(string[] tags)
    {
        float minDist = Mathf.Infinity;
        GameObject best = null;

        foreach (string tag in tags)
        {
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
            {
                if (!obj.activeInHierarchy) continue;

                float dist = Vector2.Distance(transform.position, obj.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    best = obj;
                }
            }
        }
        return best;
    }

    void MoveTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    void AdjustFacingDirection(Vector3 targetPos)
    {
        Vector3 scale = transform.localScale;
        scale.x = targetPos.x < transform.position.x ? -Mathf.Abs(originalScale.x) : Mathf.Abs(originalScale.x);
        transform.localScale = scale;
    }

    void StartBreathAttack()
    {
        isAttacking = true;
        breathTimer = breathDuration;
        PlayBreathEffect();
        StartFadeAlpha();
    }

    void EndBreathAttack()
    {
        isAttacking = false;
        StopBreathEffect();
        StartFadeAlpha();
    }

    void ApplyBreathDamage(float deltaTime)
    {
        ApplyDamageToTags(highPriorityTags, deltaTime);
        ApplyDamageToTags(midPriorityTags, deltaTime);
        ApplyDamageToTags(lowPriorityTags, deltaTime);
    }

    void ApplyDamageToTags(string[] tags, float deltaTime)
    {
        foreach (string tag in tags)
        {
            foreach (GameObject target in GameObject.FindGameObjectsWithTag(tag))
            {
                if (!target.activeInHierarchy) continue;
                if (Vector2.Distance(transform.position, target.transform.position) > breathRange) continue;

                var ally = target.GetComponent<Ally>();
                if (ally != null)
                    ally.TakeDamage(breathDamagePerSecond * deltaTime);
            }
        }
    }

    void PlayBreathEffect()
    {
        if (breathEffectPrefab == null || breathEffectSpawnPoint == null) return;
        if (currentBreathEffect != null) Destroy(currentBreathEffect);

        Quaternion rot = transform.localScale.x > 0 ? Quaternion.identity : Quaternion.Euler(0, 180f, 0);
        currentBreathEffect = Instantiate(breathEffectPrefab, breathEffectSpawnPoint.position, rot, breathEffectSpawnPoint);
    }

    void StopBreathEffect()
    {
        if (currentBreathEffect != null)
        {
            Destroy(currentBreathEffect);
            currentBreathEffect = null;
        }
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
            SetAlpha(Mathf.Lerp(1f, fadeAlpha, t));
        }
    }

    void SetAlpha(float alpha)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0f, currentHP);

        if (bossHPBar != null)
            bossHPBar.SetHP(currentHP, bossMaxHP);

        if (currentHP <= 0f)
            Die();
    }

    void Die()
    {
        Debug.Log("ドラゴン撃破");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, breathRange);
    }
}
