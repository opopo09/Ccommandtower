using UnityEngine;
using TMPro;

public class SpawnLimitChecker : MonoBehaviour
{
    [Header("制限対象タグ（複数）")]
    [SerializeField] private string[] targetTags = { "Ally", "Support", "Minion" };

    [Header("最大出現数（WaveManagerによって上書きされます）")]
    [SerializeField] private int maxTotalCount = 10;

    [Header("UI表示（省略可）")]
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private bool useUI = true;

    private void Start()
    {
        // UI更新はUpdateで行うことで、maxTotalCountの変更を即座に反映
        if (countText == null && useUI)
        {
            Debug.LogWarning("SpawnLimitChecker: countTextが設定されていません。", this.gameObject);
            useUI = false;
        }
    }

    private void Update()
    {
        if (useUI)
        {
            UpdateCountUI();
        }
    }

    /// <summary>
    /// 現在の合計数が最大数を超えていないかを返す
    /// </summary>
    public bool CanSpawn()
    {
        return GetCurrentTotalCount() < maxTotalCount;
    }

    /// <summary>
    /// 現在の合計数を取得
    /// </summary>
    public int GetCurrentTotalCount()
    {
        return GetTotalCountFromTags(targetTags);
    }

    /// <summary>
    /// UIに現在の数を反映
    /// </summary>
    private void UpdateCountUI()
    {
        if (countText != null)
        {
            int current = GetCurrentTotalCount();
            countText.text = $"Count: {current} / {maxTotalCount}";
        }
    }

    /// <summary>
    /// 【重要】外部から最大出現数を設定するメソッド
    /// </summary>
    public void SetMaxCount(int max)
    {
        maxTotalCount = max;
    }

    // --- 静的メソッド（変更なし） ---
    public static bool CanSpawnWithTag(string tag, int limit) { if (string.IsNullOrEmpty(tag)) return true; return GameObject.FindGameObjectsWithTag(tag).Length < limit; }
    public static bool CanSpawnWithTags(string[] tags, int limit) { if (tags == null || tags.Length == 0) return true; return GetTotalCountFromTags(tags) < limit; }
    private static int GetTotalCountFromTags(string[] tags) { int count = 0; foreach (var tag in tags) { if (!string.IsNullOrEmpty(tag)) count += GameObject.FindGameObjectsWithTag(tag).Length; } return count; }
}