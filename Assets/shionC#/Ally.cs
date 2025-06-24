using UnityEngine;

public class Ally : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;

    public AllyHPBar hpBar;  // Inspector�ŃZ�b�g����

    [Header("�U���ݒ�")]
    public float attackRange = 2f;           // �U���ł��鋗��
    public float attackDamage = 10f;         // �^����_���[�W
    public float attackCooldown = 1f;        // �U���Ԋu�i�b�j
    public float moveSpeed = 3f;             // �ړ����x

    [Header("�E���`�����ݒ�")]
    public float wanderRadius = 5f;          // �E���`�������锼�a
    public float wanderInterval = 3f;        // ���b���ƂɈړ����ς��邩

    private float lastAttackTime = -999f;
    private GameObject nearestEnemy;

    private Vector3 wanderTarget;
    private float wanderTimer = 0f;

    void Start()
    {
        currentHP = maxHP;
        if (hpBar != null)
            hpBar.SetHP(currentHP, maxHP);

        wanderTarget = transform.position; // �����͍��̏ꏊ
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
            // �G�Ɍ������Ĉړ�
            Vector3 dir = (nearestEnemy.transform.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
        else
        {
            // �U���\�Ȃ�U��
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
            // �V���������_���ړ�������߂�
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            wanderTarget = new Vector3(transform.position.x + randomCircle.x, transform.position.y + randomCircle.y, transform.position.z);

            wanderTimer = wanderInterval;
        }

        // wanderTarget�Ɍ������Ĉړ�
        Vector3 dir = (wanderTarget - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;

        // �ړI�n�ɋ߂Â�����^�C�}�[�����Z�b�g���ĐV�����ꏊ�����߂�悤�ɂ���
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
