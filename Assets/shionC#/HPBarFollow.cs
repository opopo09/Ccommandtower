using UnityEngine;

public class HPBarFollow : MonoBehaviour
{
    public Transform target;              // 追従する味方のTransform
    public Vector3 offset = new Vector3(0, 2.0f, 0);  // 頭上の位置調整

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 味方の位置 + オフセット に移動
        transform.position = target.position + offset;

        // カメラの方向を向かせる（ビルボード）
        if (mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }
}

