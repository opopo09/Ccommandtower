using UnityEngine;
using System.Collections.Generic;

public class DebuffArea : MonoBehaviour
{
    [Header("デバフ設定")]
    public float damagePerSecond = 10f;
    public float slowFactor = 0.3f;
    public float slowDuration = 2f;

    [Header("範囲設定")]
    public float detectionRange = 5f;
    [Range(0f, 180f)] public float attackAngle = 90f;

    [Header("ターゲット設定")]
    public string[] targetTags;

    [Header("移動設定")]
    public float moveSpeed = 2f;

    private Dictionary<GameObject, float> slowedTargets = new();
    private Dictionary<GameObject, float> originalSpeeds = new();

    void Update()
    {
        GameObject nearest = FindNearestTarget();
        if (nearest != null)
        {
            Vector3 dir = (nearest.transform.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;

            // 向き調整
            if (dir.x >= 0)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        ApplyDebuff();
        UpdateSlowTimers();
    }

    GameObject FindNearestTarget()
    {
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (string tag in targetTags)
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject t in targets)
            {
                if (!t.activeInHierarchy) continue;

                float dist = Vector2.Distance(transform.position, t.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = t;
                }
            }
        }

        return closest;
    }

    void ApplyDebuff()
    {
        foreach (string tag in targetTags)
        {
            GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject t in targets)
            {
                if (t == null || !t.activeInHierarchy) continue;

                Vector3 toTarget = t.transform.position - transform.position;
                float dist = toTarget.magnitude;
                if (dist > detectionRange) continue;

                Vector3 forward = (transform.localScale.x >= 0) ? transform.right : -transform.right;
                float angle = Vector3.Angle(forward, toTarget);
                if (angle > attackAngle / 2f) continue;

                Ally ally = t.GetComponent<Ally>();
                if (ally == null) continue;

                // ダメージ適用
                ally.TakeDamage(damagePerSecond * Time.deltaTime);

                // スロー適用
                if (!slowedTargets.ContainsKey(t))
                {
                    originalSpeeds[t] = ally.moveSpeed;
                    ally.moveSpeed *= slowFactor;
                    slowedTargets[t] = slowDuration;
                }
                else
                {
                    slowedTargets[t] = slowDuration; // timer リセット
                }
            }
        }
    }

    void UpdateSlowTimers()
    {
        List<GameObject> expired = new();

        foreach (var pair in new Dictionary<GameObject, float>(slowedTargets))
        {
            GameObject t = pair.Key;
            if (t == null)
            {
                expired.Add(t);
                continue;
            }

            slowedTargets[t] -= Time.deltaTime;

            if (slowedTargets[t] <= 0f)
            {
                Ally ally = t.GetComponent<Ally>();
                if (ally != null && originalSpeeds.ContainsKey(t))
                {
                    ally.moveSpeed = originalSpeeds[t]; // 元に戻す
                }
                expired.Add(t);
            }
        }

        foreach (GameObject t in expired)
        {
            slowedTargets.Remove(t);
            originalSpeeds.Remove(t);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Vector3 forward = (transform.localScale.x >= 0) ? transform.right : -transform.right;
        Quaternion left = Quaternion.Euler(0, 0, -attackAngle / 2f);
        Quaternion right = Quaternion.Euler(0, 0, attackAngle / 2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (left * forward) * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + (right * forward) * detectionRange);
    }
}
