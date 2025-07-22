using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer))]
public class Ally_test : MonoBehaviour // クラス名をAlly_testに変更
{
    [Header("体力設定")]
    public float maxHP = 100f;
    public AllyHPBar hpBar;

    [Header("攻撃設定")]
    public float attackRange = 2f;
    public float attackDamage = 10f;
    public float attackCooldown = 1f;
    public float moveSpeed = 3f;

    [Header("バフ設定")]
    [SerializeField] private float attackBuff = 0f; // 攻撃力に加算されるバフ値
    [SerializeField] private float hpBuff = 0f; // 最大HPに加算されるバフ値

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

    /// <summary>
    /// 移動速度の倍率を設定し、スローエフェクトの表示/非表示を切り替えます。
    /// </summary>
    /// <param name="multiplier">移動速度の倍率。</param>
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
        // 初期HPを最大HP + HPバフに設定
        currentHP = maxHP + hpBuff;
        hpBar?.SetHP(currentHP, maxHP + hpBuff); // HPバーもバフ後の最大HPを考慮

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
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(gameObject); // UnityEditor名前空間を明示
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
                    else if (nearestEnemy == null) // 初めてターゲットを見つけた場合、距離が近くなくても設定
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

    /// <summary>
    /// 敵への攻撃を試みます。攻撃力にバフ値を適用します。
    /// </summary>
    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (nearestEnemy == null) return;

        // バフ込みの攻撃力を計算
        float finalAttackDamage = attackDamage + attackBuff;

        var enemy = nearestEnemy.GetComponent<Enemy>(); // Enemyスクリプトが存在すると仮定
        if (enemy != null)
        {
            enemy.TakeDamage(finalAttackDamage); // バフ込みの攻撃力を使用
            lastAttackTime = Time.time;
            return;
        }

        var boss = nearestEnemy.GetComponent<DragonBoss>(); // DragonBossスクリプトが存在すると仮定
        if (boss != null)
        {
            boss.TakeDamage(finalAttackDamage); // バフ込みの攻撃力を使用
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

    /// <summary>
    /// ダメージを受けます。
    /// </summary>
    /// <param name="damage">受けるダメージ量。</param>
    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        hpBar?.SetHP(currentHP, maxHP + hpBuff); // HPバーもバフ後の最大HPを考慮

        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// HPを回復します。
    /// </summary>
    /// <param name="amount">回復量。</param>
    public void Heal(float amount)
    {
        currentHP += amount;
        if (currentHP > maxHP + hpBuff) // 最大HPにバフを考慮
            currentHP = maxHP + hpBuff;

        hpBar?.SetHP(currentHP, maxHP + hpBuff);
    }

    /// <summary>
    /// 攻撃力をバフします。
    /// </summary>
    /// <param name="amount">攻撃力に加算する量。</param>
    public void BuffAttack(float amount)
    {
        attackBuff += amount;
        Debug.Log($"Attack buffed by {amount}. Current attack buff: {attackBuff}");
    }

    /// <summary>
    /// HPをバフします（最大HPと現在HPに影響）。
    /// </summary>
    /// <param name="amount">HPに加算する量。</param>
    public void BuffHP(float amount)
    {
        hpBuff += amount;
        // 最大HPの増加に伴い、現在HPも割合で増加させる
        currentHP = currentHP + amount;
        if (currentHP > maxHP + hpBuff)
        {
            currentHP = maxHP + hpBuff;
        }

        hpBar?.SetHP(currentHP, maxHP + hpBuff);
        Debug.Log($"HP buffed by {amount}. Current HP buff: {hpBuff}. New max HP: {maxHP + hpBuff}");
    }

    /// <summary>
    /// 現在の攻撃力を取得します（バフ込み）。
    /// </summary>
    /// <returns>バフ込みの攻撃力。</returns>
    public float GetCurrentAttackDamage()
    {
        return attackDamage + attackBuff;
    }

    /// <summary>
    /// 現在の最大HPを取得します（バフ込み）。
    /// </summary>
    /// <returns>バフ込みの最大HP。</returns>
    public float GetCurrentMaxHP()
    {
        return maxHP + hpBuff;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}