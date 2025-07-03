using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform target;
    private float speed;
    private float damage;

    public float lifeTime = 3f;
    public float hitThreshold = 0.5f;  // ヒット判定の距離

    [Tooltip("ダメージ対象のタグ一覧")]
    public string[] damageTargetTags;  // Inspectorで設定可能

    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(Transform targetTransform, float damageAmount, float moveSpeed, string[] targetTags)
    {
        target = targetTransform;
        damage = damageAmount;
        speed = moveSpeed;
        damageTargetTags = targetTags;

        if (target == null)
        {
            Debug.LogWarning("Bullet target is null!");
        }
    }

    void Update()
    {
        if (target == null)
        {
            Debug.Log("ターゲットが消えた");
            Destroy(gameObject);
            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = target.position;
        Vector3 dir = (targetPosition - currentPosition).normalized;

        float moveDist = speed * Time.deltaTime;
        transform.position += dir * moveDist;

        if (IsLineSegmentNearPoint(lastPosition, transform.position, targetPosition, hitThreshold))
        {
            if (IsTargetTagDamageable(target.gameObject.tag))
            {
                // ダメージを与えるコンポーネント探し
                var enemy = target.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Destroy(gameObject);
                    return;
                }

                var ally = target.GetComponent<Ally>();
                if (ally != null)
                {
                    ally.TakeDamage(damage);
                    Destroy(gameObject);
                    return;
                }

                var boss = target.GetComponent<DragonBoss>();
                if (boss != null)
                {
                    boss.TakeDamage(damage);
                    Destroy(gameObject);
                    return;
                }

                Debug.LogWarning("ターゲットにダメージを与えられるコンポーネントがありません。");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("対象タグではないためダメージなし: " + target.gameObject.tag);
                Destroy(gameObject);
            }
        }

        lastPosition = currentPosition;
    }

    bool IsTargetTagDamageable(string tag)
    {
        if (damageTargetTags == null || damageTargetTags.Length == 0)
            return false;

        foreach (var t in damageTargetTags)
        {
            if (t == tag)
                return true;
        }
        return false;
    }

    bool IsLineSegmentNearPoint(Vector3 a, Vector3 b, Vector3 p, float threshold)
    {
        Vector3 ap = p - a;
        Vector3 ab = b - a;

        float abSqr = ab.sqrMagnitude;
        if (abSqr == 0f) return Vector3.Distance(a, p) <= threshold;

        float t = Mathf.Clamp01(Vector3.Dot(ap, ab) / abSqr);
        Vector3 closestPoint = a + t * ab;

        return Vector3.Distance(closestPoint, p) <= threshold;
    }
}
