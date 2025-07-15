using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DebuffEnemy : MonoBehaviour
{
    [Header("デバフ設定")]
    public float damagePerSecond = 10f;
    public float slowFactor = 0.5f;
    public float debuffDuration = 1.5f;

    [Header("攻撃範囲")]
    public float effectRadius = 2f;

    [Header("攻撃角度")]
    [Range(0f, 360f)]
    public float attackAngle = 90f;

    [Header("移動設定")]
    public float moveSpeed = 2f;
    public float stopDistance = 1.5f;

    [Header("ターゲット設定")]
    public string[] targetTags;

    // 攻撃方向（向き）を別で管理（初期は右向き）
    public Vector2 attackDirection = Vector2.right;

    private GameObject currentTarget;
    private Dictionary<Ally, float> debuffTimers = new Dictionary<Ally, float>();

    void Update()
    {
        FindNearestTarget();

        if (currentTarget != null)
        {
            Vector3 dir = currentTarget.transform.position - transform.position;

            float dist = dir.magnitude;

            if (dist > stopDistance)
            {
                MoveTowards(currentTarget.transform.position);

                // 攻撃方向をターゲット方向に向ける（向きは変えず、攻撃方向だけ更新）
                attackDirection = new Vector2(dir.x, dir.y).normalized;
            }
            else
            {
                // 攻撃範囲内なら攻撃方向は固定のままでもOK
            }

            ApplyAreaDebuff();
        }

        UpdateDebuffTimers();
    }

    void FindNearestTarget()
    {
        float closest = Mathf.Infinity;
        GameObject nearest = null;

        foreach (string tag in targetTags)
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in targets)
            {
                float dist = Vector3.Distance(transform.position, obj.transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    nearest = obj;
                }
            }
        }

        currentTarget = nearest;
    }

    void MoveTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    void ApplyAreaDebuff()
    {
        foreach (string tag in targetTags)
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in targets)
            {
                Vector2 toTarget = (Vector2)(obj.transform.position - transform.position);

                float dist = toTarget.magnitude;
                if (dist > effectRadius) continue;

                // attackDirectionで角度判定
                float angleToTarget = Vector2.Angle(attackDirection, toTarget.normalized);

                if (angleToTarget > attackAngle / 2f) continue;

                Ally ally = obj.GetComponent<Ally>();
                if (ally != null)
                {
                    if (!debuffTimers.ContainsKey(ally))
                    {
                        ally.SetMoveSpeedMultiplier(slowFactor);
                    }
                    debuffTimers[ally] = debuffDuration;

                    ally.TakeDamage(damagePerSecond * Time.deltaTime);
                }
            }
        }
    }

    void UpdateDebuffTimers()
    {
        var expired = new List<Ally>();

        foreach (var ally in debuffTimers.Keys.ToList())
        {
            if (ally == null)
            {
                expired.Add(ally);
                continue;
            }

            float timer = debuffTimers[ally] - Time.deltaTime;

            if (timer <= 0f)
            {
                expired.Add(ally);
            }
            else
            {
                debuffTimers[ally] = timer;
            }
        }

        foreach (Ally ally in expired)
        {
            ally.SetMoveSpeedMultiplier(1f);
            debuffTimers.Remove(ally);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, effectRadius);

        // 攻撃方向を可視化
        Vector3 dir = new Vector3(attackDirection.x, attackDirection.y, 0) * effectRadius;
        float halfAngle = attackAngle / 2f;

        Vector3 leftDir = Quaternion.Euler(0, 0, -halfAngle) * dir;
        Vector3 rightDir = Quaternion.Euler(0, 0, halfAngle) * dir;

        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + leftDir);
        Gizmos.DrawLine(transform.position, transform.position + rightDir);
    }
}
