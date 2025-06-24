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

    [Header("ウロチョロ設定")]
    public float wanderRadius = 5f;          // ウロチョロする半径
    public float wanderInterval = 3f;        // 何秒ごとに移動先を変えるか

    private float lastAttackTime = -999f;
    private GameObject nearestEnemy;

    private Vector3 wanderTarget;
    private float wanderTimer = 0f;

    void Start()
    {
        currentHP = maxHP;
        if (hpBar != null)
            hpBar.SetHP(currentHP, maxHP);

        wanderTarget = transform.position; // 初期は今の場所
    }

    void Update()
    {
        FindNearestEnemy();

        if (nearestEnemy == null)
        {
            Wander();
            return;
        }

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

    void Wander()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f)
        {
            // 新しいランダム移動先を決める
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            wanderTarget = new Vector3(transform.position.x + randomCircle.x, transform.position.y + randomCircle.y, transform.position.z);

            wanderTimer = wanderInterval;
        }

        // wanderTargetに向かって移動
        Vector3 dir = (wanderTarget - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        // 目的地に近づいたらタイマーをリセットして新しい場所を決めるようにする
        if (Vector3.Distance(transform.position, wanderTarget) < 0.1f)
        {
            wanderTimer = 0f;
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
