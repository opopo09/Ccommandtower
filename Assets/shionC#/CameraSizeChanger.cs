using UnityEngine;

public class CameraSizeChanger : MonoBehaviour
{
    public Camera targetCamera;
    public float size = 5f;

    void Start()
    {
        if (targetCamera != null)
        {
            targetCamera.orthographicSize = size;
        }
    }
}
