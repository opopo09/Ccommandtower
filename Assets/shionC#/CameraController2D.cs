using UnityEngine;

public class FreeLookCamera2D : MonoBehaviour
{
    public Transform target;             // 追従対象（nullなら自由移動）
    public float rightStickSpeed = 5f;   // 右スティック視点移動速度
    public float mouseDragSpeed = 0.1f;  // マウスドラッグ速度

    public Vector2 minPosition;  // カメラの移動制限（最小XY）
    public Vector2 maxPosition;  // カメラの移動制限（最大XY）

    [Header("ズーム設定")]
    public float zoomSpeed = 5f;            // ズーム速度
    public float minOrthographicSize = 3f;  // 最小ズーム
    public float maxOrthographicSize = 10f; // 最大ズーム

    private Vector3 cameraPos;
    private bool isDragging = false;
    private Vector3 lastMousePos;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            Debug.LogError("Cameraコンポーネントがアタッチされていません");

        cameraPos = transform.position;
    }

    void Update()
    {
        if (target != null)
        {
            // ターゲットにピタッと追従
            cameraPos = target.position;
        }
        else
        {
            // 右スティックで視点移動
            float rightX = Input.GetAxis("RightStickHorizontal");
            float rightY = Input.GetAxis("RightStickVertical");
            Vector3 move = new Vector3(rightX, rightY, 0f) * rightStickSpeed * Time.deltaTime;
            cameraPos += move;

            // マウス右ドラッグ
            if (Input.GetMouseButtonDown(1))
            {
                isDragging = true;
                lastMousePos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                isDragging = false;
            }
            if (isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePos;
                cameraPos -= new Vector3(delta.x, delta.y, 0f) * mouseDragSpeed;
                lastMousePos = Input.mousePosition;
            }

            // 移動範囲制限
            cameraPos.x = Mathf.Clamp(cameraPos.x, minPosition.x, maxPosition.x);
            cameraPos.y = Mathf.Clamp(cameraPos.y, minPosition.y, maxPosition.y);
        }

        // 左スティック縦軸でズーム調整
        float leftStickY = Input.GetAxis("Vertical"); // 通常"Vertical"が左スティックの上下入力
        if (cam != null && Mathf.Abs(leftStickY) > 0.01f)
        {
            cam.orthographicSize -= leftStickY * zoomSpeed * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthographicSize, maxOrthographicSize);
        }

        // マウスホイールでもズーム可能（優先度低め）
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        if (cam != null && Mathf.Abs(mouseWheel) > 0.01f)
        {
            cam.orthographicSize -= mouseWheel * zoomSpeed * 50f * Time.deltaTime; // 感度少しアップ
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthographicSize, maxOrthographicSize);
        }
    }

    void LateUpdate()
    {
        cameraPos.z = -10f; // 2DカメラはZ固定
        transform.position = cameraPos;
    }
}
