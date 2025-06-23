using UnityEngine;

public class FreeLookCamera2D : MonoBehaviour
{
    public Transform target;             // �Ǐ]�Ώہinull�Ȃ玩�R�ړ��j
    public float rightStickSpeed = 5f;   // �E�X�e�B�b�N���_�ړ����x
    public float mouseDragSpeed = 0.1f;  // �}�E�X�h���b�O���x

    public Vector2 minPosition;  // �J�����̈ړ������i�ŏ�XY�j
    public Vector2 maxPosition;  // �J�����̈ړ������i�ő�XY�j

    [Header("�Y�[���ݒ�")]
    public float zoomSpeed = 5f;            // �Y�[�����x
    public float minOrthographicSize = 3f;  // �ŏ��Y�[��
    public float maxOrthographicSize = 10f; // �ő�Y�[��

    private Vector3 cameraPos;
    private bool isDragging = false;
    private Vector3 lastMousePos;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            Debug.LogError("Camera�R���|�[�l���g���A�^�b�`����Ă��܂���");

        cameraPos = transform.position;
    }

    void Update()
    {
        if (target != null)
        {
            // �^�[�Q�b�g�Ƀs�^�b�ƒǏ]
            cameraPos = target.position;
        }
        else
        {
            // �E�X�e�B�b�N�Ŏ��_�ړ�
            float rightX = Input.GetAxis("RightStickHorizontal");
            float rightY = Input.GetAxis("RightStickVertical");
            Vector3 move = new Vector3(rightX, rightY, 0f) * rightStickSpeed * Time.deltaTime;
            cameraPos += move;

            // �}�E�X�E�h���b�O
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

            // �ړ��͈͐���
            cameraPos.x = Mathf.Clamp(cameraPos.x, minPosition.x, maxPosition.x);
            cameraPos.y = Mathf.Clamp(cameraPos.y, minPosition.y, maxPosition.y);
        }

        // ���X�e�B�b�N�c���ŃY�[������
        float leftStickY = Input.GetAxis("Vertical"); // �ʏ�"Vertical"�����X�e�B�b�N�̏㉺����
        if (cam != null && Mathf.Abs(leftStickY) > 0.01f)
        {
            cam.orthographicSize -= leftStickY * zoomSpeed * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthographicSize, maxOrthographicSize);
        }

        // �}�E�X�z�C�[���ł��Y�[���\�i�D��x��߁j
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        if (cam != null && Mathf.Abs(mouseWheel) > 0.01f)
        {
            cam.orthographicSize -= mouseWheel * zoomSpeed * 50f * Time.deltaTime; // ���x�����A�b�v
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minOrthographicSize, maxOrthographicSize);
        }
    }

    void LateUpdate()
    {
        cameraPos.z = -10f; // 2D�J������Z�Œ�
        transform.position = cameraPos;
    }
}
