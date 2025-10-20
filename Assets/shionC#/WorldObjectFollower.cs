using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class WorldObjectFollower : MonoBehaviour
{
    private Slider slider;
    private Transform target;
    private Vector3 offset;
    private Camera mainCamera;

    void Awake()
    {
        slider = GetComponent<Slider>();
        mainCamera = Camera.main;
    }

    // 他のスクリプトから、追跡対象とオフセットを設定してもらうためのメソッド
    public void Initialize(Transform followTarget, Vector3 worldOffset)
    {
        target = followTarget;
        offset = worldOffset;
    }

    // UpdateではなくLateUpdateを使い、カメラの移動が完了した後に座標計算を行う
    void LateUpdate()
    {
        if (target == null || mainCamera == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // 追跡対象のワールド座標（＋オフセット）を、スクリーン座標に変換
        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position + offset);

        // オブジェクトがカメラの後ろにある場合は非表示にする
        if (screenPos.z < 0)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            transform.position = screenPos;
        }
    }

    // 他のスクリプトから、スライダーの値を更新してもらうためのメソッド
    public void UpdateValue(float current, float max)
    {
        if (slider != null)
        {
            slider.maxValue = max;
            slider.value = current;
        }
    }
}