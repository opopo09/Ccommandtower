using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("çUåÇê›íË")]
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    private float lastAttackTime = -999f;

    [Header("É^Å[ÉQÉbÉgê›íË")]
    public string[] superPriorityTags;   // ç≈óDêÊÇ≈ë_Ç§É^ÉOÅièáî‘Ç™èdóvÅj
    public string[] priorityTags;        // óDêÊìIÇ…ë_Ç§É^ÉOÅièáî‘Ç™èdóvÅj
    public string[] targetTags;          // çUåÇëŒè€Ç∆ÇµÇƒîFéØÇ∑ÇÈÉ^ÉO

    [Header("íeÇÃê›íË")]
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

        // á@ ç≈óDêÊÉ^ÉOÇ©ÇÁèáÇ…íTÇ∑
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
                if (best != null) return best; // ç≈óDêÊÇÃëŒè€Ç™å©Ç¬Ç©Ç¡ÇΩÇÁë¶ï‘Ç∑
            }
        }

        // áA óDêÊÉ^ÉOÇ©ÇÁèáÇ…íTÇ∑
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
                if (best != null) return best; // óDêÊÇÃëŒè€Ç™å©Ç¬Ç©Ç¡ÇΩÇÁë¶ï‘Ç∑
            }
        }

        // áB fallback: ëSëÃÇÃ targetTags Ç©ÇÁãﬂÇ¢èáÇ≈íTÇ∑
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
            // TowerÇÃtargetTagsîzóÒÇíeÇ…ìnÇ∑
            bulletScript.Initialize(target.transform, bulletDamage, bulletSpeed, targetTags);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
