using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private string sceneNameToLoad = "NextScene"; // 遷移先のシーン名

    void Update()
    {
        // XboxのAボタンが押されたらシーンを切り替える
        if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            SceneManager.LoadScene(sceneNameToLoad);
        }
    }
}
