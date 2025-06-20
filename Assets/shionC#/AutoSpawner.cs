using UnityEngine;

public class AutoSpawner : MonoBehaviour
{
    [Header("�X�|�[���ݒ�")]
    public GameObject prefabToSpawn;     // ���ł��X�|�[���ł���v���n�u
    public Transform spawnPoint;         // �o���ʒu
    public float spawnInterval = 3f;     // �X�|�[���Ԋu�i�b�j

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
