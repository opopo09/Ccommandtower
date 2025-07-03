using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("�U���ݒ�")]
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    private float lastAttackTime = -999f;

    [Header("�^�[�Q�b�g�ݒ�")]
    public string[] superPriorityTags;   // �ŗD��ő_���^�O�i���Ԃ��d�v�j
    public string[] priorityTags;        // �D��I�ɑ_���^�O�i���Ԃ��d�v�j
    public string[] targetTags;          // �U���ΏۂƂ��ĔF������^�O

    [Header("�e�̐ݒ�")]
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

        // �@ �ŗD��^�O���珇�ɒT��
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
                if (best != null) return best; // �ŗD��̑Ώۂ����������瑦�Ԃ�
            }
        }

        // �A �D��^�O���珇�ɒT��
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
                if (best != null) return best; // �D��̑Ώۂ����������瑦�Ԃ�
            }
        }

        // �B fallback: �S�̂� targetTags ����߂����ŒT��
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
            // Tower��targetTags�z���e�ɓn��
            bulletScript.Initialize(target.transform, bulletDamage, bulletSpeed, targetTags);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
