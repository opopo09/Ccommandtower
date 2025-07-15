using UnityEngine;
using TMPro;

public class EnemyTower : MonoBehaviour
{
    [Header("çUåÇê›íË")]
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    private float lastAttackTime = -999f;

    [Header("à⁄ìÆê›íË")]
    public float moveSpeed = 2f;

    [Header("íeÇÃê›íË")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 5f;  // íeÇÃë¨ìxÇÕè≠ÇµíxÇﬂÇ…í≤êÆçœÇ›
    public float bulletDamage = 10f;
    public int maxAmmo = 10;
    public int currentAmmo = 10;

    [Header("íeâÒïúê›íË")]
    public float ammoRecoveryCooldown = 3f;
    private float lastAmmoRecoveryTime = -999f;
    public int ammoRecoveryAmount = 1;

    [Header("É^Å[ÉQÉbÉgê›íË")]
    public string[] targetTags;

    [Header("UIê›íË")]
    public TextMeshProUGUI ammoText;

    private GameObject currentTarget;

    void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    void Update()
    {
        // íeÇÃé©ìÆâÒïúèàóù
        if (Time.time - lastAmmoRecoveryTime >= ammoRecoveryCooldown && currentAmmo < maxAmmo)
        {
            currentAmmo = Mathf.Min(currentAmmo + ammoRecoveryAmount, maxAmmo);
            lastAmmoRecoveryTime = Time.time;
            UpdateAmmoUI();
        }

        FindTarget();

        if (currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

            if (dist <= attackRange)
            {
                if (Time.time - lastAttackTime >= attackCooldown && currentAmmo > 0)
                {
                    ShootBullet(currentTarget);
                    lastAttackTime = Time.time;
                    currentAmmo--;
                    UpdateAmmoUI();
                }
            }
            else
            {
                MoveToTarget(currentTarget);
            }
        }
    }

    void FindTarget()
    {
        GameObject nearest = null;
        float closestDist = Mathf.Infinity;

        foreach (string tag in targetTags)
        {
            var candidates = GameObject.FindGameObjectsWithTag(tag);
            foreach (var c in candidates)
            {
                float dist = Vector3.Distance(transform.position, c.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    nearest = c;
                }
            }
        }

        currentTarget = nearest;
    }

    void MoveToTarget(GameObject target)
    {
        if (target == null) return;
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void ShootBullet(GameObject target)
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();  // ÇøÇ·ÇÒÇ∆EnemyBulletÇéÊìæÅI
        if (bulletScript != null)
        {
            bulletScript.Initialize(target.transform, bulletDamage, bulletSpeed, targetTags);
        }
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = $"Ammo: {currentAmmo}/{maxAmmo}";
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
