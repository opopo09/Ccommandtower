using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // public変数は削除または残してもOK（初期値参照用なら残す）
    // public float speed = 2f;
    // public float damage = 10f;
    public float attackCooldown = 1f;
    public float attackRange = 1.5f;

    [Header("ターゲットタグ")]
    public string allyTag = "Ally";
    public string baseTag = "Base";

    private Transform currentTarget;
    private float lastAttackTime = -999f;
    private Enemy enemy;  // HPなどと攻撃力や速度も管理するコンポーネント

    void Start()
    {
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("Enemy コンポーネントがありません");
        }
    }

    void Update()
    {
        if (enemy == null) return;

        GameObject[] allies = GameObject.FindGameObjectsWithTag(allyTag);
        currentTarget = GetClosestTarget(allies);

        // 味方がいなければ拠点を探す
        if (currentTarget == null)
        {
            GameObject baseObj = GameObject.FindGameObjectWithTag(baseTag);
            if (baseObj != null)
            {
                currentTarget = baseObj.transform;
            }
        }

        if (currentTarget == null) return;

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        if (distance > attackRange)
        {
            Vector3 dir = (currentTarget.position - transform.position).normalized;
            // Enemyコンポーネントのspeedを使う
            transform.position += dir * enemy.speed * Time.deltaTime;

            // スプライトの向きを調整（右向き前提）
            if (dir.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(dir.x);
                transform.localScale = scale;
            }
        }
        else
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Ally ally = currentTarget.GetComponent<Ally>();
                BaseHP baseHP = currentTarget.GetComponent<BaseHP>();

                if (ally != null)
                {
                    // Enemyコンポーネントのdamageを使う
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

    Transform GetClosestTarget(GameObject[] targets)
    {
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject obj in targets)
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = obj.transform;
            }
        }

        return closest;
    }
}
