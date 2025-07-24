using UnityEngine;

public class HealingArea : MonoBehaviour
{
    [Header("回復設定")]
    public float healAmountPerSecond = 5f;
    public float healRadius = 3f;
    public float duration = 5f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(gameObject); // 時間経過で消滅
            return;
        }

        HealAlliesInRange();
    }

    void HealAlliesInRange()
    {
        // 新しいAPI（高速・非推奨回避）
        Ally[] allies = Object.FindObjectsByType<Ally>(FindObjectsSortMode.None);

        foreach (var ally in allies)
        {
            float dist = Vector3.Distance(transform.position, ally.transform.position);
            if (dist <= healRadius)
            {
                ally.Heal(healAmountPerSecond * Time.deltaTime);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRadius); // 範囲の可視化
    }
}
