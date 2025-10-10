using UnityEngine;

public class HorizontalUnitSpawner : MonoBehaviour
{
    [Header("生成するユニット")]
    public GameObject unitPrefab; // SimpleMoverが付いた、横切るユニットのプレハブ

    [Header("生成設定")]
    public float spawnInterval = 5f; // 生成する間隔（秒）
    public float initialSpawnDelay = 1f; // 最初の生成までの時間

    [Header("ユニットの挙動設定")]
    public float unitSpeed = 3f; // ユニットの移動速度
    public bool movesRight = true; // trueなら右、falseなら左に移動
    public float unitLifetime = 10f; // ユニットが自動で消えるまでの時間

    private float timer;

    void Start()
    {
        // すべてのSpawnerが同時に生成しないように、最初の時間を少しずらす
        timer = Random.Range(0, initialSpawnDelay);
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            SpawnUnit();
            timer = spawnInterval; // タイマーをリセット
        }
    }

    void SpawnUnit()
    {
        if (unitPrefab == null) return;

        // 自分の位置にユニットを生成
        GameObject newUnit = Instantiate(unitPrefab, transform.position, Quaternion.identity);

        // 生成したユニットに付いているSimpleMoverコンポーネントを取得
        SimpleMover mover = newUnit.GetComponent<SimpleMover>();
        if (mover != null)
        {
            // SimpleMoverに必要な情報を渡してあげる
            mover.speed = this.unitSpeed;
            mover.direction = this.movesRight ? Vector2.right : Vector2.left;
        }

        // 指定した時間が経過したらユニットを自動で破壊する
        Destroy(newUnit, unitLifetime);
    }
}