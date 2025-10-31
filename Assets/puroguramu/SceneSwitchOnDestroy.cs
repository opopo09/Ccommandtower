using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitchOnDestroy : MonoBehaviour
{
    [Header("監視対象のオブジェクト")]
    public GameObject targetObject;

    [Header("遷移先のシーン名")]
    public string nextSceneName;

    void Update()
    {
        if (targetObject == null)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
