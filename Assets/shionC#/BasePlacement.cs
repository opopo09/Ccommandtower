// ファイル名: BasePlacement.cs
using UnityEngine;
using UnityEngine.Events;

public class BasePlacement : MonoBehaviour
{
    [Header("設置する基地のプレハブ")]
    public GameObject basePrefab;
    [Header("基地を設置するカメラ")]
    public Camera mainCamera;
    [System.Serializable]
    public class BasePlacedEvent : UnityEvent<Transform> { }
    public BasePlacedEvent onBasePlaced;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

            // Z座標にカメラからの正しい距離を設定
            screenCenter.z = Mathf.Abs(mainCamera.transform.position.z);

            Vector3 worldCenter = mainCamera.ScreenToWorldPoint(screenCenter);
            worldCenter.z = 0; // 念のためZ=0平面に固定

            GameObject newBase = Instantiate(basePrefab, worldCenter, Quaternion.identity);
            onBasePlaced.Invoke(newBase.transform);
            this.enabled = false;
        }
    }
}