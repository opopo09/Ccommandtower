using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyAI : MonoBehaviour
{
    public float attackCooldown = 1f;
    public float attackRange = 1.5f;

    [Header("ターゲット設定")]
    public string[] highPriorityTags;  // 最優先
    public string[] midPriorityTags;   // 中優先
    public string[] lowPriorityTags;   // 通常優先

    private Transform currentTarget;
    private float lastAttackTime = -999f;
    private Enemy enemy;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (enemy == null)
        {
            Debug.LogError("Enemy コンポーネントがありません");
        }
    }

    void Update()
    {
        if (enemy == null) return;

        // ターゲット探し（優先度順）
        currentTarget = FindClosestTarget(highPriorityTags)
                     ?? FindClosestTarget(midPriorityTags)
                     ?? FindClosestTarget(lowPriorityTags);

        if (currentTarget == null) return;

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        if (distance > attackRange)
        {
            Vector3 dir = (currentTarget.position - transform.position).normalized;
            transform.position += dir * enemy.speed * Time.deltaTime;

            // 画像だけ反転（子オブジェクトには影響なし）
            if (spriteRenderer != null && Mathf.Abs(dir.x) > 0.01f)
            {
                spriteRenderer.flipX = dir.x < 0;
            }
        }
        else
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                // 攻撃処理
                Ally ally = currentTarget.GetComponent<Ally>();
                BaseHP baseHP = currentTarget.GetComponent<BaseHP>();

                if (ally != null)
                {
                    ally.TakeDamage(enemy.damage);
                }
                else if (baseHP != null)
                {
                    baseHP.TakeDamage(enemy.damage);
                }

                lastAttackTime = Time.time;
            }
        }
    }

    Transform FindClosestTarget(string[] tags)
    {
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (string tag in tags)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objs)
            {
                if (!obj.activeInHierarchy) continue;

                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = obj.transform;
                }
            }
        }

        return closest;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
