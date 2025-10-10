using UnityEngine;

public class ObstacleData : MonoBehaviour
{
    [Tooltip("AIがこのオブジェクトを避ける円の半径")]
    public float avoidanceRadius = 1.0f;

    // Sceneビューで回避範囲を視覚的に確認するためのGizmo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.5f, 0, 0.5f); // オレンジ色
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
    }
}