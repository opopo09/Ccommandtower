using UnityEngine;

public class LTViewSwitcher : MonoBehaviour
{
    [Header("�J�����ݒ�")]
    public Camera normalCam;
    public Camera[] ltCameras;

    [Header("Canvas�ݒ�")]
    public Canvas[] normalCanvasList;
    public Canvas[] ltCanvasList;

    [Header("���͐ݒ�")]
    public string ltAxisName = "LT";        // LT�g���K�[�iInput Manager�Őݒ�j
    public string dpadYAxisName = "DPadY";  // �\���L�[�㉺�iInput Manager�Őݒ�j

    private int currentLTIndex = 0;
    private bool canChangeView = true;

    void Update()
    {
        float ltValue = Input.GetAxis(ltAxisName);
        bool isLT = ltValue > 0.1f;

        if (isLT)
        {
            HandleDPadSwitch();
            SetLTView(currentLTIndex);
        }
        else
        {
            currentLTIndex = 0;
            SetNormalView();
            canChangeView = true;
        }
    }

    void HandleDPadSwitch()
    {
        float vertical = Input.GetAxisRaw(dpadYAxisName); // �� �\���L�[�̏㉺����

        if (canChangeView && Mathf.Abs(vertical) > 0.5f)
        {
            if (vertical > 0)
            {
                currentLTIndex = (currentLTIndex - 1 + ltCameras.Length) % ltCameras.Length;
            }
            else
            {
                currentLTIndex = (currentLTIndex + 1) % ltCameras.Length;
            }

            canChangeView = false;
        }

        if (Mathf.Abs(vertical) < 0.1f)
        {
            canChangeView = true;
        }
    }

    void SetLTView(int index)
    {
        if (normalCam != null)
            normalCam.enabled = false;

        for (int i = 0; i < ltCameras.Length; i++)
        {
            if (ltCameras[i] != null)
                ltCameras[i].enabled = (i == index);
        }

        foreach (var canvas in normalCanvasList)
        {
            if (canvas != null)
                canvas.enabled = false;
        }

        for (int i = 0; i < ltCanvasList.Length; i++)
        {
            if (ltCanvasList[i] != null)
                ltCanvasList[i].enabled = (i == index);
        }
    }

    void SetNormalView()
    {
        if (normalCam != null)
            normalCam.enabled = true;

        foreach (var cam in ltCameras)
        {
            if (cam != null)
                cam.enabled = false;
        }

        foreach (var canvas in normalCanvasList)
        {
            if (canvas != null)
                canvas.enabled = true;
        }

        foreach (var canvas in ltCanvasList)
        {
            if (canvas != null)
                canvas.enabled = false;
        }
    }
}
