using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("攻撃設定")]
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    private float lastAttackTime = -999f;

    [Header("ターゲット設定")]
    public string[] superPriorityTags;   // 最優先で狙うタグ（順番が重要）
    public string[] priorityTags;        // 優先的に狙うタグ（順番が重要）
    public string[] targetTags;          // 攻撃対象として認識するタグ

    [Header("弾の設定")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public float bulletDamage = 10f;

    void Update()
    {
        GameObject target = FindTarget();
        if (target != null && Time.time - lastAttackTime >= attackCooldown)
        {
            ShootBullet(target);
            lastAttackTime = Time.time;
        }
    }

    GameObject FindTarget()
    {
        GameObject best = null;
        float closestDist = Mathf.Infinity;

        // �@ 最優先タグから順に探す
        if (superPriorityTags != null)
        {
            foreach (string tag in superPriorityTags)
            {
                GameObject[] candidates = GameObject.FindGameObjectsWithTag(tag);
                foreach (var c in candidates)
                {
                    if (!c.activeInHierarchy) continue;
                    float dist = Vector3.Distance(transform.position, c.transform.position);
                    if (dist <= attackRange && dist < closestDist)
                    {
                        closestDist = dist;
                        best = c;
                    }
                }
                if (best != null) return best; // 最優先の対象が見つかったら即返す
            }
        }

        // �A 優先タグから順に探す
        if (priorityTags != null)
        {
            foreach (string tag in priorityTags)
            {
                GameObject[] candidates = GameObject.FindGameObjectsWithTag(tag);
                foreach (var c in candidates)
                {
                    if (!c.activeInHierarchy) continue;
                    float dist = Vector3.Distance(transform.position, c.transform.position);
                    if (dist <= attackRange && dist < closestDist)
                    {
                        closestDist = dist;
                        best = c;
                    }
                }
                if (best != null) return best; // 優先の対象が見つかったら即返す
            }
        }

        // �B fallback: 全体の targetTags から近い順で探す
        if (targetTags != null)
        {
            foreach (string tag in targetTags)
            {
                GameObject[] candidates = GameObject.FindGameObjectsWithTag(tag);
                foreach (var c in candidates)
                {
                    if (!c.activeInHierarchy) continue;
                    float dist = Vector3.Distance(transform.position, c.transform.position);
                    if (dist <= attackRange && dist < closestDist)
                    {
                        closestDist = dist;
                        best = c;
                    }
                }
            }
        }

        return best;
    }

    void ShootBullet(GameObject target)
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            // TowerのtargetTags配列を弾に渡す
            bulletScript.Initialize(target.transform, bulletDamage, bulletSpeed, targetTags);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
