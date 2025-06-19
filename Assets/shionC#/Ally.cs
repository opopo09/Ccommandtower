using UnityEngine;

public class Ally : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;

    public AllyHPBar hpBar;  // Inspector‚ÅƒZƒbƒg‚·‚é

    [Header("UŒ‚İ’è")]
    public float attackRange = 2f;           // UŒ‚‚Å‚«‚é‹——£
    public float attackDamage = 10f;         // —^‚¦‚éƒ_ƒ[ƒW
    public float attackCooldown = 1f;        // UŒ‚ŠÔŠui•bj
    public float moveSpeed = 3f;             // ˆÚ“®‘¬“x

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
            // “G‚ÉŒü‚©‚Á‚ÄˆÚ“®
            Vector3 dir = (nearestEnemy.transform.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
        else
        {
            // UŒ‚‰Â”\‚È‚çUŒ‚
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






