using UnityEngine;

public class HPBarFollow : MonoBehaviour
{
    public Transform target;              // �Ǐ]���閡����Transform
    public Vector3 offset = new Vector3(0, 2.0f, 0);  // ����̈ʒu����

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // �����̈ʒu + �I�t�Z�b�g �Ɉړ�
        transform.position = target.position + offset;

        // �J�����̕�������������i�r���{�[�h�j
        if (mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }
}

