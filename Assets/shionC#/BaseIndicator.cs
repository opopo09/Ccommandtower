using UnityEngine;

public class BaseIndicator : MonoBehaviour
{
    public Transform baseTransform;        // 追跡したい基地オブジェクト
    public RectTransform indicatorUI;      // Canvas内の矢印UI（RectTransform）
    public Camera mainCamera;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (indicatorUI != null)
            indicatorUI.gameObject.SetActive(false);
    }

    void Update()
    {
        if (baseTransform == null || indicatorUI == null) return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(baseTransform.position);

        // 画面内か判定
        bool onScreen = viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;

        if (onScreen)
        {
            indicatorUI.gameObject.SetActive(false);
        }
        else
        {
            indicatorUI.gameObject.SetActive(true);

            // カメラの後ろにいるなら補正
            if (viewportPos.z < 0)
            {
                viewportPos.x = 1 - viewportPos.x;
                viewportPos.y = 1 - viewportPos.y;
                viewportPos.z = 0;
            }

            // 画面端の少し内側に矢印を表示
            viewportPos.x = Mathf.Clamp(viewportPos.x, 0.05f, 0.95f);
            viewportPos.y = Mathf.Clamp(viewportPos.y, 0.05f, 0.95f);

            Vector3 screenPos = mainCamera.ViewportToScreenPoint(viewportPos);

            // indicatorUIはCanvasの子でスクリーンスペース・オーバーレイ想定なのでそのままセット
            indicatorUI.position = screenPos;

            // 中心から矢印の向きを計算（時計回りで角度調整）
            Vector3 dir = (screenPos - new Vector3(Screen.width / 2f, Screen.height / 2f)).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            indicatorUI.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }
}
