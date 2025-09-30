using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    // public Transform target; // ### 変更点: public変数は不要になるので削除

    private Transform mainCameraTransform; // ### 変更点: メインカメラを保持する変数

    void Start()
    {
        // ### 変更点: メインカメラを自動で探して設定する
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Main Cameraが見つかりません！カメラに'MainCamera'タグが付いているか確認してください。");
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform == null) return;

        // ### 変更点: メインカメラのX,Y座標に、自身のZ座標を合わせて追従する
        Vector3 targetPosition = mainCameraTransform.position;
        transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
    }
}