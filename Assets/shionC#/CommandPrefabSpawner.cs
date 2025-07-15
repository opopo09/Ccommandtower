using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CommandUIController : MonoBehaviour
{
    [Header("コマンド設定")]
    public List<string> commandSequence = new List<string> { "A", "B", "X" };

    [Header("表示するUIプレハブ（Canvas用）")]
    public GameObject uiPrefab;

    [Header("キャンバス")]
    public Canvas canvas;

    [Header("操作キー設定")]
    public KeyCode buttonA = KeyCode.JoystickButton0;
    public KeyCode buttonB = KeyCode.JoystickButton1;
    public KeyCode buttonX = KeyCode.JoystickButton2;

    [Header("コマンド入力リセット時間（秒）")]
    public float inputResetTime = 2f;

    [Header("音声設定")]
    public AudioClip buttonPressSE;
    public AudioSource audioSource;

    private int inputIndex = 0;
    private float resetTimer = 0f;
    private GameObject previewInstance;

    void Start()
    {
        if (canvas == null)
        {
            Debug.LogError("Canvasがセットされていません！");
            enabled = false;
            return;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // CanvasのRenderMode確認（警告だけ）
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.renderMode != RenderMode.ScreenSpaceCamera)
        {
            Debug.LogWarning("CanvasのRender Modeは Screen Space Overlay か Screen Space Camera にしてください。");
        }
    }

    void Update()
    {
        HandleCommandInput();
        UpdateResetTimer();
    }

    void HandleCommandInput()
    {
        if (Input.GetKeyDown(buttonA)) ProcessInput("A");
        else if (Input.GetKeyDown(buttonB)) ProcessInput("B");
        else if (Input.GetKeyDown(buttonX)) ProcessInput("X");
    }

    void ProcessInput(string input)
    {
        PlayButtonSE();

        if (inputIndex >= commandSequence.Count)
        {
            ResetCommand();
            return;
        }

        if (commandSequence[inputIndex] == input)
        {
            inputIndex++;
            resetTimer = inputResetTime;

            if (inputIndex >= commandSequence.Count)
            {
                SpawnPreview();
                ResetCommand();
            }
        }
        else
        {
            ResetCommand();
        }
    }

    void PlayButtonSE()
    {
        if (buttonPressSE != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonPressSE);
        }
    }

    void ResetCommand()
    {
        inputIndex = 0;
        resetTimer = 0f;
    }

    void UpdateResetTimer()
    {
        if (inputIndex == 0) return;

        resetTimer -= Time.deltaTime;
        if (resetTimer <= 0f)
        {
            ResetCommand();
        }
    }

    void SpawnPreview()
    {
        if (uiPrefab == null)
        {
            Debug.LogWarning("uiPrefabがセットされていません");
            return;
        }

        if (previewInstance != null)
        {
            Destroy(previewInstance);
        }

        previewInstance = Instantiate(uiPrefab, canvas.transform);

        RectTransform rt = previewInstance.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }
        else
        {
            previewInstance.transform.localPosition = Vector3.zero;
        }
    }
}
