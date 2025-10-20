using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Transform target;
    private float speed;
    private float damage;

    public float lifeTime = 3f;
    public float hitThreshold = 0.5f;

    [Tooltip("ダメージ対象のタグ一覧")]
    public string[] damageTargetTags;

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
            // ターゲットが破壊されるなどして消えた場合
            Destroy(gameObject);
            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = target.position;
        Vector3 dir = (targetPosition - currentPosition).normalized;

        // ▼▼▼▼▼【ここからが新しいAIの核心部分です】▼▼▼▼▼
        // 弾の向きをターゲットの方向へ向ける
        if (dir != Vector3.zero) // ゼロベクトルでなければ（稀なケースですが安全のため）
        {
            // 方向ベクトル(x, y)から、アークタンジェントを使って角度(ラジアン)を計算
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Unityの回転(Quaternion)に変換し、Z軸周りに回転させる
            // スプライトが元々「上向き」に描かれている場合、-90度のオフセットが必要
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 360f);
        }
        // ▲▲▲▲▲【ここまで】▲▲▲▲▲

        float moveDist = speed * Time.deltaTime;
        transform.position += dir * moveDist;

        if (IsLineSegmentNearPoint(lastPosition, transform.position, targetPosition, hitThreshold))
        {
            if (IsTargetTagDamageable(target.gameObject.tag))
            {
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

                Debug.LogWarning("ターゲットにダメージを与えられるコンポーネントがありませんでした。");
                Destroy(gameObject);
            }
            else
            {
                // 対象タグではないが、ヒットはしたので弾は消滅
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