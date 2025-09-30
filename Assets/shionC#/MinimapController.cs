using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapController : MonoBehaviour
{
    [Header("カメラの設定")]
    public Camera minimapCamera; // ミニマップ専用のカメラをここに設定

    [Header("アイコンの設定")]
    public GameObject allyIconPrefab;
    public GameObject enemyIconPrefab;
    [Space(10)]
    public GameObject allyOffscreenIconPrefab;
    public GameObject enemyOffscreenIconPrefab;

    [Header("ミニマップのUI設定")]
    public RectTransform minimapRect;
    public Transform iconParent;

    // --- プライベート変数 ---
    private Transform cameraTransform;
    private List<GameObject> spawnedIcons = new List<GameObject>();
    private float minimapRadius;

    // ワールド座標をミニマップ座標に変換するための、正しい縮尺
    private Vector2 worldToMinimapScale;

    void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Main Cameraが見つかりません！");
        }

        if (minimapRect == null || minimapCamera == null)
        {
            Debug.LogError("Minimap Rect または Minimap Camera が設定されていません！");
            return;
        }

        // ミニマップUIの半径を計算
        minimapRadius = minimapRect.rect.width / 2.0f;

        // --- 最重要：正しい縮尺の計算 ---
        // 1. ミニマップカメラが映しているワールド空間での高さを取得
        float worldHeight = minimapCamera.orthographicSize * 2.0f;
        // 2. その高さとアスペクト比から、ワールド空間での幅を計算
        float worldWidth = worldHeight * minimapCamera.aspect;

        // 3. ワールドの大きさとUIの大きさの比率を計算
        worldToMinimapScale.x = minimapRect.rect.width / worldWidth;
        worldToMinimapScale.y = minimapRect.rect.height / worldHeight;
    }

    void LateUpdate()
    {
        // 古いアイコンを削除
        foreach (GameObject icon in spawnedIcons)
        {
            Destroy(icon);
        }
        spawnedIcons.Clear();

        if (cameraTransform == null) return;

        UpdateIconsForTag("Ally", allyIconPrefab, allyOffscreenIconPrefab);
        UpdateIconsForTag("Enemy", enemyIconPrefab, enemyOffscreenIconPrefab);
    }

    private void UpdateIconsForTag(string tag, GameObject inScreenPrefab, GameObject offScreenPrefab)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject target in targets)
        {
            // メインカメラからの相対的なワールド座標を計算
            Vector3 offset = target.transform.position - cameraTransform.position;

            // 正しい縮尺を使って、ワールド座標をミニマップUI座標に変換
            Vector2 newPosition = new Vector2(offset.x * worldToMinimapScale.x, offset.y * worldToMinimapScale.y);

            if (newPosition.magnitude > minimapRadius)
            {
                // --- 画面外の処理 ---
                Vector2 clampedPosition = newPosition.normalized * minimapRadius;
                GameObject newIcon = Instantiate(offScreenPrefab);
                newIcon.transform.SetParent(iconParent, false);
                newIcon.GetComponent<RectTransform>().anchoredPosition = clampedPosition;
                float angle = Mathf.Atan2(newPosition.y, newPosition.x) * Mathf.Rad2Deg;
                newIcon.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, angle - 90f);
                spawnedIcons.Add(newIcon);
            }
            else
            {
                // --- 画面内の処理 ---
                GameObject newIcon = Instantiate(inScreenPrefab);
                newIcon.transform.SetParent(iconParent, false);
                newIcon.GetComponent<RectTransform>().anchoredPosition = newPosition;
                spawnedIcons.Add(newIcon);
            }
        }
    }
}