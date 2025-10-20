using UnityEngine;
using System.Collections.Generic;

public class OffScreenIndicatorManager : MonoBehaviour
{
    public static OffScreenIndicatorManager instance;

    [Header("設定 (最重要)")]
    [Tooltip("UIインジケーターのプレハブ")]
    public GameObject indicatorPrefab;
    [Tooltip("UIを配置するCanvasのTransform")]
    public RectTransform canvasRect;

    [Header("表示の調整")]
    [Tooltip("画面の端から、インジケーターをどれだけ内側に表示するか")]
    public float screenMargin = 50f;

    private Camera mainCamera;
    private List<EnemyAI> trackedEnemies = new List<EnemyAI>();
    private Dictionary<EnemyAI, GameObject> indicatorDict = new Dictionary<EnemyAI, GameObject>();

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (indicatorPrefab == null || canvasRect == null) return;
        trackedEnemies.RemoveAll(item => item == null);
        foreach (var enemy in trackedEnemies)
        {
            if (enemy == null) continue;
            bool isOffScreen = IsTargetOffScreen(enemy.transform, out Vector3 screenPos);
            bool isFighting = enemy.IsAttacking;
            if (isOffScreen && isFighting)
            {
                if (!indicatorDict.ContainsKey(enemy))
                {
                    GameObject newIndicator = Instantiate(indicatorPrefab, canvasRect);
                    indicatorDict[enemy] = newIndicator;
                }
                UpdateIndicator(indicatorDict[enemy], screenPos);
            }
            else
            {
                if (indicatorDict.ContainsKey(enemy))
                {
                    if (indicatorDict[enemy] != null) indicatorDict[enemy].SetActive(false);
                }
            }
        }
    }

    private void UpdateIndicator(GameObject indicator, Vector3 screenPosition)
    {
        if (indicator == null) return;
        indicator.SetActive(true);
        indicator.transform.position = screenPosition;
        Vector3 center = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Vector3 dir = (screenPosition - center).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        indicator.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    private bool IsTargetOffScreen(Transform target, out Vector3 screenPosition)
    {
        if (target == null || mainCamera == null) { screenPosition = Vector3.zero; return false; }
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(target.position);
        if (viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1)
        {
            screenPosition = Vector3.zero;
            return false;
        }
        else
        {
            Vector3 clampedViewportPos = viewportPos;
            clampedViewportPos.x = Mathf.Clamp01(clampedViewportPos.x);
            clampedViewportPos.y = Mathf.Clamp01(clampedViewportPos.y);
            screenPosition = mainCamera.ViewportToScreenPoint(clampedViewportPos);
            screenPosition.x = Mathf.Clamp(screenPosition.x, screenMargin, Screen.width - screenMargin);
            screenPosition.y = Mathf.Clamp(screenPosition.y, screenMargin, Screen.height - screenMargin);
            return true;
        }
    }

    public void AddEnemy(EnemyAI enemy) { if (enemy != null && !trackedEnemies.Contains(enemy)) trackedEnemies.Add(enemy); }
    public void RemoveEnemy(EnemyAI enemy)
    {
        if (enemy != null)
        {
            if (indicatorDict.ContainsKey(enemy)) { if (indicatorDict[enemy] != null) Destroy(indicatorDict[enemy]); indicatorDict.Remove(enemy); }
            if (trackedEnemies.Contains(enemy)) trackedEnemies.Remove(enemy);
        }
    }
}