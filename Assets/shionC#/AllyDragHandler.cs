using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AllyDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject allyPrefab; // �h���b�O��ŏo�����j�b�g
    private GameObject dragIcon;  // UI��Ń}�E�X�ɂ������C���[�W
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

        // UI�̏�ł͂Ȃ��ꏊ�ɗ��Ƃ����������o���i�C�Ӂj
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // �J��������}�E�X�ʒu�����[���h�ɕϊ��iZ��C�ӂ̋����ɌŒ�j
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10f; // �J��������̋����i�������Ăˁj

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePosition);
            Instantiate(allyPrefab, worldPos, Quaternion.identity);
        }
    }
}