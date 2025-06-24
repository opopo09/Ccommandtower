using UnityEngine;

public class BaseIndicator : MonoBehaviour
{
    public Transform baseTransform;        // �ǐՂ�������n�I�u�W�F�N�g
    public RectTransform indicatorUI;      // Canvas���̖��UI�iRectTransform�j
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

        // ��ʓ�������
        bool onScreen = viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;

        if (onScreen)
        {
            indicatorUI.gameObject.SetActive(false);
        }
        else
        {
            indicatorUI.gameObject.SetActive(true);

            // �J�����̌��ɂ���Ȃ�␳
            if (viewportPos.z < 0)
            {
                viewportPos.x = 1 - viewportPos.x;
                viewportPos.y = 1 - viewportPos.y;
                viewportPos.z = 0;
            }

            // ��ʒ[�̏��������ɖ���\��
            viewportPos.x = Mathf.Clamp(viewportPos.x, 0.05f, 0.95f);
            viewportPos.y = Mathf.Clamp(viewportPos.y, 0.05f, 0.95f);

            Vector3 screenPos = mainCamera.ViewportToScreenPoint(viewportPos);

            // indicatorUI��Canvas�̎q�ŃX�N���[���X�y�[�X�E�I�[�o�[���C�z��Ȃ̂ł��̂܂܃Z�b�g
            indicatorUI.position = screenPos;

            // ���S������̌������v�Z�i���v���Ŋp�x�����j
            Vector3 dir = (screenPos - new Vector3(Screen.width / 2f, Screen.height / 2f)).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            indicatorUI.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }
}
