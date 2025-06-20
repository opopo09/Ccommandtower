using UnityEngine;

public class AutoSpawner : MonoBehaviour
{
    [Header("スポーン設定")]
    public GameObject prefabToSpawn;     // 何でもスポーンできるプレハブ
    public Transform spawnPoint;         // 出現位置
    public float spawnInterval = 3f;     // スポーン間隔（秒）

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnObject();
            timer = 0f;
        }
    }

    void SpawnObject()
    {
        if (prefabToSpawn != null && spawnPoint != null)
        {
            Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
