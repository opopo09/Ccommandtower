using UnityEngine;

public class LegacyCommandSpawner : MonoBehaviour
{
    public GameObject previewPrefab;  // UI表示用（Canvas内）
    public GameObject spawnPrefab;    // フィールドに出すやつ
    public Transform spawnPoint;      // 展開位置

    private GameObject previewInstance;
    private int currentIndex = 0;

    private KeyCode[] commandSequence = new KeyCode[]
    {
        KeyCode.JoystickButton0, // A
        KeyCode.JoystickButton1, // B
        KeyCode.JoystickButton3, // Y
        KeyCode.JoystickButton2  // X
    };

    void Update()
    {
        if (Input.GetKeyDown(commandSequence[currentIndex]))
        {
            currentIndex++;
            if (currentIndex >= commandSequence.Length)
            {
                ShowPreview();
            }
        }
        else if (AnyOtherButtonPressed())
        {
            currentIndex = 0;
            HidePreview();
        }

        if (previewInstance != null && Input.GetKeyDown(KeyCode.JoystickButton7)) // RTボタン
        {
            Spawn();
            HidePreview();
            currentIndex = 0;
        }
    }

    void ShowPreview()
    {
        if (previewInstance == null && previewPrefab != null)
        {
            previewInstance = Instantiate(previewPrefab);
            var canvas = FindFirstObjectByType<Canvas>();

            if (canvas != null)
            {
                previewInstance.transform.SetParent(canvas.transform, false); // ←これ重要！
            }
            else
            {
                Debug.LogWarning("Canvas が見つかりません");
            }
        }
    }

    void HidePreview()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }
    }

    void Spawn()
    {
        if (spawnPrefab != null && spawnPoint != null)
        {
            Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    bool AnyOtherButtonPressed()
    {
        for (int i = 0; i <= 15; i++)
        {
            KeyCode key = KeyCode.JoystickButton0 + i;
            if (System.Array.Exists(commandSequence, c => c == key)) continue;

            if (Input.GetKeyDown(key))
                return true;
        }
        return false;
    }
}
