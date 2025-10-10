using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    [Header("生成する射出装置")]
    public GameObject spawnerPrefab; // HorizontalUnitSpawnerが付いたプレハブ

    [Header("配置設定")]
    public int numberOfSpawners = 5; // 配置する射出装置の数

    [Header("円の範囲設定")]
    public Vector2 center = Vector2.zero; // 円の中心座標
    public float radius = 10f; // 円の半径

    void Start()
    {
        if (spawnerPrefab == null)
        {
            Debug.LogError("Spawner Prefabが設定されていません！");
            return;
        }

        SpawnSpawners();
    }

    void SpawnSpawners()
    {
        for (int i = 0; i < numberOfSpawners; i++)
        {
            // 円の中のランダムな座標を生成
            Vector2 randomPoint = center + Random.insideUnitCircle * radius;

            // 射出装置のプレハブを、計算したランダムな位置に生成
            // this.transformを親にすることで、Hierarchyが整理される
            Instantiate(spawnerPrefab, randomPoint, Quaternion.identity, this.transform);
        }
    }

    // Sceneビューに円の範囲を視覚的に表示するためのGizmo
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(center.x, center.y, 0), radius);
    }
}