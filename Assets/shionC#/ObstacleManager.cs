using UnityEngine;
using System.Collections.Generic;

public class ObstaclePlacer : MonoBehaviour
{
    // ▼▼▼▼▼【ここからが変更点です】▼▼▼▼▼
    [Header("配置する障害物")]
    [Tooltip("ここに複数の障害物プレハブを設定できます。ランダムに選ばれて配置されます。")]
    public GameObject[] obstaclePrefabs; // 単一のGameObjectから、GameObjectの配列に変更
    // ▲▲▲▲▲【ここまで】▲▲▲▲▲

    [Header("配置設定")]
    [Tooltip("円の中に配置する障害物の数")]
    public int numberOfObstacles = 50;

    [Header("円の範囲設定")]
    public Vector2 center = Vector2.zero;
    [Tooltip("この半径の外側には障害物を生成しません（外側の境界）")]
    public float outerRadius = 15f;
    [Tooltip("この半径の内側には障害物を生成しません（内側の境界）")]
    public float innerRadius = 5f;

    [Header("配置の調整")]
    [Tooltip("障害物同士がこの距離より近づかないように配置します")]
    public float minDistanceBetweenObstacles = 1.0f;

    void Start()
    {
        PlaceObstacles();
    }

    void PlaceObstacles()
    {
        // ▼▼▼▼▼【ここからが変更点です】▼▼▼▼▼
        // プレハブが一つも設定されていないかチェック
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogError("障害物のプレハブが一つも設定されていません！");
            return;
        }
        // ▲▲▲▲▲【ここまで】▲▲▲▲▲

        List<Vector2> spawnedPositions = new List<Vector2>();

        for (int i = 0; i < numberOfObstacles; i++)
        {
            int attempts = 0;

            while (attempts < 100)
            {
                Vector2 randomPoint = center + Random.insideUnitCircle * outerRadius;

                if (Vector2.Distance(randomPoint, center) < innerRadius)
                {
                    attempts++;
                    continue;
                }

                bool isTooClose = false;
                foreach (Vector2 pos in spawnedPositions)
                {
                    if (Vector2.Distance(randomPoint, pos) < minDistanceBetweenObstacles)
                    {
                        isTooClose = true;
                        break;
                    }
                }

                if (!isTooClose)
                {
                    // ▼▼▼▼▼【ここからが変更点です】▼▼▼▼▼
                    // 設定されたプレハブのリストから、ランダムに一つを選ぶ
                    GameObject randomPrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

                    // 選ばれたランダムなプレハブを生成する
                    Instantiate(randomPrefab, randomPoint, Quaternion.identity, this.transform);
                    // ▲▲▲▲▲【ここまで】▲▲▲▲▲

                    spawnedPositions.Add(randomPoint);
                    break;
                }

                attempts++;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector3(center.x, center.y, 0), outerRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(center.x, center.y, 0), innerRadius);
    }
}