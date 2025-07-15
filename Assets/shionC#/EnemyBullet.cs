using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Transform target;
    private float speed;
    private float damage;
    private string[] damageTargetTags;

    public float lifeTime = 3f;
    public float hitThreshold = 0.7f;

    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
        Destroy(gameObject, lifeTime);
    }

    // 弾の初期化
    public void Initialize(Transform targetTransform, float damageAmount, float moveSpeed, string[] targetTags)
    {
        target = targetTransform;
        damage = damageAmount;
        speed = moveSpeed;
        damageTargetTags = targetTags;

        if (target == null)
        {
            Debug.LogWarning("EnemyBullet: ターゲットが設定されていません！");
        }
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = target.position;
        Vector3 direction = (targetPosition - currentPosition).normalized;

        float moveDistance = speed * Time.deltaTime;
        transform.position += direction * moveDistance;

        // 移動前後の線分とターゲット位置の距離判定で当たり判定
        if (IsLineSegmentNearPoint(lastPosition, transform.position, targetPosition, hitThreshold))
        {
            ApplyDamage(target.gameObject);
            Destroy(gameObject);
            return;
        }

        lastPosition = currentPosition;
    }

    void ApplyDamage(GameObject hitObject)
    {
        if (!IsTargetTagDamageable(hitObject.tag))
        {
            Debug.Log($"EnemyBullet: タグ不一致でダメージ無効: {hitObject.tag}");
            return;
        }

        var ally = hitObject.GetComponent<Ally>();
        if (ally != null)
        {
            ally.TakeDamage(damage);
            Debug.Log($"EnemyBullet: {damage} ダメージを {hitObject.name} に与えた");
            return;
        }

        Debug.LogWarning("EnemyBullet: ダメージ対象のコンポーネントが見つかりません");
    }

    bool IsTargetTagDamageable(string tag)
    {
        if (damageTargetTags == null || damageTargetTags.Length == 0) return false;

        foreach (var t in damageTargetTags)
        {
            if (t == tag) return true;
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
