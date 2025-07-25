using UnityEngine;
using TMPro;

public class SpawnLimitChecker : MonoBehaviour
{
    [Header("制限対象タグ（複数）")]
    [SerializeField] private string[] targetTags = { "Ally", "Support", "Minion" };

    [Header("最大出現数")]
    [SerializeField] private int maxTotalCount = 10;

    [Header("UI表示")]
    [SerializeField] private TextMeshProUGUI countText;

    private int previousCount = -1;

    private void Start()
    {
        // 0.2秒ごとにUIを更新
        InvokeRepeating(nameof(UpdateCountUI), 0f, 0.2f);
    }

    /// <summary>
    /// targetTags の合計数が maxTotalCount を超えていないかを返す（インスタンス用）
    /// </summary>
    public bool CanSpawn()
    {
        return GetCurrentTotalCount() < maxTotalCount;
    }

    /// <summary>
    /// 現在の合計数を取得
    /// </summary>
    private int GetCurrentTotalCount()
    {
        return GetTotalCountFromTags(targetTags);
    }

    /// <summary>
    /// UIに現在の数を反映
    /// </summary>
    private void UpdateCountUI()
    {
        if (countText == null) return;

        int current = GetCurrentTotalCount();
        if (current != previousCount)
        {
            countText.text = $"Count: {current} / {maxTotalCount}";
            previousCount = current;
        }
    }

    /// <summary>
    /// 静的に単一タグで出現数をチェック
    /// </summary>
    public static bool CanSpawnWithTag(string tag, int limit)
    {
        if (string.IsNullOrEmpty(tag)) return true;

        int count = GameObject.FindGameObjectsWithTag(tag).Length;
        return count < limit;
    }

    /// <summary>
    /// 静的に複数タグの合計数をチェック
    /// </summary>
    public static bool CanSpawnWithTags(string[] tags, int limit)
    {
        if (tags == null || tags.Length == 0) return true;

        int total = GetTotalCountFromTags(tags);
        return total < limit;
    }

    /// <summary>
    /// 指定タグ群の総数を取得（共通処理）
    /// </summary>
    private static int GetTotalCountFromTags(string[] tags)
    {
        int count = 0;

        foreach (var tag in tags)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                count += GameObject.FindGameObjectsWithTag(tag).Length;
            }
        }

        return count;
    }
}
