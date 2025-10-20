using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

    [Header("SE設定")]
    public AudioClip attackSound;
    public AudioClip deathSound;
    [Range(0.1f, 3f)] public float deathSoundVolume = 2.0f;

    [Header("ターゲット設定（優先度）")]
    public string[] priorityTargetTagsLv1;
    public string[] priorityTargetTagsLv2;
    public string[] priorityTargetTagsLv3;

    [Header("スローエフェクト")]
    public GameObject slowEffectPrefab;
    public Vector3 slowEffectOffset = Vector3.zero;

    [Header("反転設定")]
    public GameObject flipPrefab;

    private float currentHP;
    private float lastAttackTime = -999f;
    private GameObject nearestEnemy;
    private SpriteRenderer spriteRenderer;
    private float moveSpeedMultiplier = 1f;
    private bool isSlowed = false;
    private GameObject slowEffectInstance;
    private bool allowFlip = false;
    private Animator animator;
    private AudioSource audioSource;
    private bool isDying = false;

    private Queue<Vector3> path;
    private float pathRequestCooldown = 1.0f;
    private float lastPathRequestTime = -999f;
    private Transform currentTarget;

    void Start()
    {
        currentHP = maxHP;
        hpBar?.SetHP(currentHP, maxHP);
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (flipPrefab == null) { allowFlip = true; }
#if UNITY_EDITOR
        else { var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject); allowFlip = prefab == flipPrefab; }
#else
        else { allowFlip = gameObject.name.Contains(flipPrefab.name); }
#endif
    }

    void Update()
    {
        if (isDying) return;

        FindNearestEnemy();

        if (nearestEnemy == null)
        {
            // 敵がいない場合、移動を停止
            path?.Clear();
        }
        else
        {
            if (Vector3.Distance(transform.position + attackCenterOffset, nearestEnemy.transform.position) <= attackRange)
            {
                // 攻撃範囲内なら攻撃
                TryAttack();
                path?.Clear(); // 攻撃中は移動計画をクリア
            }
            else
            {
                // 攻撃範囲外なら、追跡する
                ChaseTarget();
            }
        }
    }

    void ChaseTarget()
    {
        if (nearestEnemy == null) return;

        if (currentTarget != nearestEnemy.transform || (path == null || path.Count == 0))
        {
            if (Time.time > lastPathRequestTime + pathRequestCooldown)
            {
                currentTarget = nearestEnemy.transform;
                lastPathRequestTime = Time.time;
                path = EnemyAI.RequestPathFromAI(transform.position, currentTarget.position);
            }
        }

        FollowPath();
    }

    void FollowPath()
    {
        if (path == null || path.Count == 0) return;
        Vector3 currentWaypoint = path.Peek();
        MoveAndFace(currentWaypoint);
        if (Vector3.Distance(transform.position, currentWaypoint) < 0.2f)
        {
            path.Dequeue();
        }
    }

    void FindNearestEnemy()
    {
        Vector3 centerPos = transform.position + attackCenterOffset;
        nearestEnemy = null;
        float minDistance = float.MaxValue;
        System.Action<string[]> findAction = (tags) => { if (tags == null) return; foreach (string tag in tags) { if (string.IsNullOrEmpty(tag)) continue; try { GameObject[] targets = GameObject.FindGameObjectsWithTag(tag); foreach (GameObject target in targets) { float dist = Vector3.Distance(centerPos, target.transform.position); if (dist < minDistance) { minDistance = dist; nearestEnemy = target; } } } catch (UnityException) { continue; } } };
        findAction(priorityTargetTagsLv1);
        if (nearestEnemy == null) findAction(priorityTargetTagsLv2);
        if (nearestEnemy == null) findAction(priorityTargetTagsLv3);
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        if (nearestEnemy == null) return;
        if (audioSource != null && attackSound != null) { audioSource.PlayOneShot(attackSound); }
        if (animator != null) { animator.SetTrigger("Attack"); }

        var enemy = nearestEnemy.GetComponent<Enemy>();
        if (enemy != null) { enemy.TakeDamage(attackDamage); lastAttackTime = Time.time; return; }
        var boss = nearestEnemy.GetComponent<DragonBoss>();
        if (boss != null) { boss.TakeDamage(attackDamage); lastAttackTime = Time.time; }
    }

    void MoveAndFace(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction == Vector3.zero) return;
        if (allowFlip && spriteRenderer != null) { spriteRenderer.flipX = direction.x < 0; }
        transform.position += direction * moveSpeed * moveSpeedMultiplier * Time.deltaTime;
    }

    public void TakeDamage(float damage)
    {
        if (isDying) return;
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        hpBar?.SetHP(currentHP, maxHP);
        if (currentHP <= 0) { Die(); }
    }

    void Die()
    {
        isDying = true;
        if (audioSource != null && deathSound != null) { audioSource.PlayOneShot(deathSound, deathSoundVolume); }
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (hpBar != null) hpBar.gameObject.SetActive(false);
        this.enabled = false;
        float destroyDelay = (deathSound != null) ? deathSound.length : 0.5f;
        Destroy(gameObject, destroyDelay);
    }

    public void Heal(float amount)
    {
        if (isDying) return;
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        hpBar?.SetHP(currentHP, maxHP);
    }

    public void SetMoveSpeedMultiplier(float multiplier)
    {
        moveSpeedMultiplier = multiplier;
        bool nowSlowed = multiplier < 1f;
        if (nowSlowed && !isSlowed) StartSlowEffect();
        else if (!nowSlowed && isSlowed) StopSlowEffect();
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
        if (slowEffectInstance != null) { Destroy(slowEffectInstance); slowEffectInstance = null; }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + attackCenterOffset, attackRange);
        if (path != null && path.Count > 0)
        {
            Gizmos.color = Color.green;
            Vector3 prev = transform.position;
            foreach (var p in path)
            {
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }
    }
}