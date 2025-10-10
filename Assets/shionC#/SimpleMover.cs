using UnityEngine;

public class SimpleMover : MonoBehaviour
{
    // HorizontalUnitSpawnerから設定される変数
    public float speed;
    public Vector2 direction;

    void Update()
    {
        // 指定された方向に、指定された速度で移動するだけ
        transform.Translate(direction * speed * Time.deltaTime);
    }
}