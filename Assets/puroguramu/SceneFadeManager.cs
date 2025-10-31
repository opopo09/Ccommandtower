using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFadeManager : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1.0f;
    public string sceneNameToLoad = "NextScene";

    private bool isFading = false;

    void Start()
    {
        // ŠJŽnŽž‚ÉImage‚ðŠ®‘S‚É“§–¾‚É
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            fadeImage.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    void Update()
    {
        if (!isFading && Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            StartCoroutine(FadeAndLoadScene());
        }
    }

    IEnumerator FadeAndLoadScene()
    {
        isFading = true;

        float timer = 0f;
        Color c = fadeImage.color;

        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = new Color(c.r, c.g, c.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(c.r, c.g, c.b, 1f);
        SceneManager.LoadScene(sceneNameToLoad);
    }
}
