using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AllyDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject allyPrefab; // ドラッグ先で出すユニット
    private GameObject dragIcon;  // UI上でマウスにくっつくイメージ
    private Canvas canvas;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragIcon = new GameObject("DragIcon", typeof(Image));
        dragIcon.transform.SetParent(canvas.transform, false);
        dragIcon.transform.SetAsLastSibling();

        Image iconImage = dragIcon.GetComponent<Image>();
        iconImage.sprite = GetComponent<Image>().sprite;
        iconImage.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }

        // UIの上ではない場所に落とした時だけ出す（任意）
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // カメラからマウス位置をワールドに変換（Zを任意の距離に固定）
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10f; // カメラからの距離（調整してね）

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePosition);
            Instantiate(allyPrefab, worldPos, Quaternion.identity);
        }
    }
}