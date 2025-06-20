using UnityEngine;

public class Ally : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;

    public AllyHPBar hpBar;  // Inspectorでセットする

    [Header("攻撃設定")]
    public float attackRange = 2f;           // 攻撃できる距離
    public float attackDamage = 10f;         // 与えるダメージ
    public float attackCooldown = 1f;        // 攻撃間隔（秒）
    public float moveSpeed = 3f;             // 移動速度

    private float lastAttackTime = -999f;
    private GameObject nearestEnemy;

    void Start()
    {
        currentHP = maxHP;
        if (hpBar != null)
            hpBar.SetHP(currentHP, maxHP);
    }

    void Update()
    {
        FindNearestEnemy();

        if (nearestEnemy == null) return;

        float dist = Vector3.Distance(transform.position, nearestEnemy.transform.position);

        if (dist > attackRange)
        {
            // 敵に向かって移動
            Vector3 dir = (nearestEnemy.transform.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
        else
        {
            // 攻撃可能なら攻撃
            TryAttack();
        }
    }

    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            nearestEnemy = null;
            return;
        }

        float minDist = Mathf.Infinity;
        GameObject closest = null;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }

        nearestEnemy = closest;
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (nearestEnemy == null) return;

        Enemy enemyScript = nearestEnemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        if (hpBar != null)
            hpBar.SetHP(currentHP, maxHP);

        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }
}






