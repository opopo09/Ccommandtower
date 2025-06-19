using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 2f;
    public float damage = 10f;
    public float attackCooldown = 1f;
    public float attackRange = 1.5f;

    public Transform baseTarget; // 拠点のTransform
    private Transform currentTarget;

    private float lastAttackTime = -999f;
    private Enemy enemy;  // HPなどを管理するコンポーネント

    void Start()
    {
        enemy = GetComponent<Enemy>();
        if (enemy == null)
        {
            Debug.LogError("Enemyコンポーネントがありません");
        }
    }

    void Update()
    {
        if (enemy == null) return;

        GameObject[] allies = GameObject.FindGameObjectsWithTag("Ally");
        currentTarget = GetClosestTarget(allies);

        if (currentTarget == null)
        {
            currentTarget = baseTarget;
        }

        if (currentTarget == null) return;

        float distance = Vector3.Distance(transform.position, currentTarget.position);

        if (distance > attackRange)
        {
            Vector3 dir = (currentTarget.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }
        else
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Ally ally = currentTarget.GetComponent<Ally>();
                BaseHP baseHP = currentTarget.GetComponent<BaseHP>();

                if (ally != null)
                {
                    ally.TakeDamage(damage);
                }
                else if (baseHP != null)
                {
                    baseHP.TakeDamage(damage);
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


