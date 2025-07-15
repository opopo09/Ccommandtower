using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class Castle : MonoBehaviour
{
    [Header("スポーンする歩兵プレハブ")]
    public GameObject infantryPrefab;

    [Header("スポーンする歩兵の数")]
    [Range(1, 20)]
    public int spawnCount = 1;

    [Header("スポーン位置のオフセット")]
    public Vector3 spawnOffset = new Vector3(1, 0, 0);

    [Header("コマンドの猶予時間（秒） 0で無制限")]
    public float commandTimeout = 0f;

    [Header("コマンドステップ数（ランダム生成時）")]
    [Range(1, 10)]
    public int commandStepCount = 2;

    [Header("ミスできる回数（0で即リセット）")]
    public int allowedMistakes = 1;

    [Header("コマンド画像を表示する親")]
    public Transform commandImagePanel;

    [Header("ボタン画像のプレハブ (Image付き)")]
    public GameObject commandImagePrefab;

    [Header("各ボタンのスプライト")]
    public Sprite spriteA;
    public Sprite spriteB;
    public Sprite spriteX;
    public Sprite spriteY;
    public Sprite spriteUp;
    public Sprite spriteDown;
    public Sprite spriteLeft;
    public Sprite spriteRight;

    [Header("正解入力時のSE")]
    public AudioClip inputSE;

    [Header("ミス時のSE")]
    public AudioClip mistakeSE;

    [Header("カスタムコマンドを使用するか")]
    public bool useCustomCommand = false;

    [Header("手動で設定するコマンド（0:A, 1:B, 2:X, 3:Y, 4:↑, 5:↓, 6:←, 7:→）")]
    public List<int> presetCommand = new List<int>();

    private AudioSource audioSource;
    private List<int> currentCommand = new List<int>();
    private int currentStep = 0;
    private float commandStartTime = 0f;
    private List<GameObject> commandImages = new List<GameObject>();
    private int currentMistakes = 0;
    private bool inputLocked = false;

    private float prevDpadX = 0f;
    private float prevDpadY = 0f;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        if (useCustomCommand && presetCommand.Count > 0)
        {
            UsePresetCommand();
        }
        else
        {
            GenerateRandomCommand();
        }
    }

    void Update()
    {
        if (currentCommand.Count == 0) return;

        if (commandStartTime > 0 && commandTimeout > 0 && Time.time - commandStartTime > commandTimeout)
        {
            Debug.Log("コマンドタイムアウト（入力後）");
            ResetCommand();
            return;
        }

        // A/B/X/Yボタン
        for (int i = 0; i <= 3; i++)
        {
            if (Input.GetKeyDown("joystick button " + i))
            {
                HandleInput(i);
                return;
            }
        }

        // DPad入力（長押し防止）
        float dpadX = Input.GetAxisRaw("DPadX");
        float dpadY = Input.GetAxisRaw("DPadY");

        if (!inputLocked)
        {
            if (prevDpadY <= 0.5f && dpadY > 0.5f)
            {
                HandleInput(4); // ↑
                StartCoroutine(LockInput());
            }
            else if (prevDpadY >= -0.5f && dpadY < -0.5f)
            {
                HandleInput(5); // ↓
                StartCoroutine(LockInput());
            }
            else if (prevDpadX >= -0.5f && dpadX < -0.5f)
            {
                HandleInput(6); // ←
                StartCoroutine(LockInput());
            }
            else if (prevDpadX <= 0.5f && dpadX > 0.5f)
            {
                HandleInput(7); // →
                StartCoroutine(LockInput());
            }
        }

        prevDpadX = dpadX;
        prevDpadY = dpadY;
    }

    void HandleInput(int input)
    {
        int expected = currentCommand[currentStep];

        if (commandStartTime == 0f)
        {
            commandStartTime = Time.time;
            Debug.Log("入力開始：タイマーをスタート");
        }

        if (input == expected)
        {
            Debug.Log($"Step {currentStep + 1}: 入力 {input} 正解");
            HighlightCommandStep(currentStep);

            if (inputSE != null) audioSource.PlayOneShot(inputSE);

            currentStep++;

            if (currentStep >= currentCommand.Count)
            {
                StartCoroutine(CompleteCommandSequence());
            }
        }
        else
        {
            currentMistakes++;
            Debug.LogWarning($"間違った入力！ミス: {currentMistakes}/{allowedMistakes}");

            StartCoroutine(FlashWrongCommand(currentStep));

            if (mistakeSE != null) audioSource.PlayOneShot(mistakeSE);

            if (allowedMistakes == 0 || currentMistakes >= allowedMistakes)
            {
                Debug.Log("ミス上限でリセット");
                ResetCommand();
            }
        }
    }

    IEnumerator LockInput()
    {
        inputLocked = true;
        yield return new WaitForSeconds(0.1f);
        inputLocked = false;
    }

    IEnumerator CompleteCommandSequence()
    {
        yield return new WaitForSeconds(0.1f);
        SpawnInfantry();

        if (useCustomCommand && presetCommand.Count > 0)
            UsePresetCommand();
        else
            GenerateRandomCommand();
    }

    void GenerateRandomCommand()
    {
        currentCommand.Clear();
        ClearCommandImages();

        for (int i = 0; i < commandStepCount; i++)
        {
            int randomBtn = Random.Range(0, 8);
            currentCommand.Add(randomBtn);
            AddCommandImage(randomBtn);
        }

        currentStep = 0;
        currentMistakes = 0;
        commandStartTime = 0f;
    }

    void UsePresetCommand()
    {
        currentCommand.Clear();
        ClearCommandImages();

        foreach (int btn in presetCommand)
        {
            currentCommand.Add(btn);
            AddCommandImage(btn);
        }

        currentStep = 0;
        currentMistakes = 0;
        commandStartTime = 0f;
    }

    void AddCommandImage(int btn)
    {
        GameObject imgObj = Instantiate(commandImagePrefab, commandImagePanel);
        Image img = imgObj.GetComponent<Image>();
        img.sprite = GetSpriteForButton(btn);
        commandImages.Add(imgObj);
    }

    void ClearCommandImages()
    {
        foreach (GameObject obj in commandImages)
        {
            Destroy(obj);
        }
        commandImages.Clear();
    }

    void HighlightCommandStep(int step)
    {
        if (step < commandImages.Count)
        {
            Image img = commandImages[step].GetComponent<Image>();
            img.color = Color.gray;
        }
    }

    IEnumerator FlashWrongCommand(int step)
    {
        if (step < commandImages.Count)
        {
            Image img = commandImages[step].GetComponent<Image>();
            Color originalColor = img.color;

            for (int i = 0; i < 2; i++)
            {
                img.color = Color.red;
                yield return new WaitForSeconds(0.15f);
                img.color = originalColor;
                yield return new WaitForSeconds(0.15f);
            }
        }
    }

    Sprite GetSpriteForButton(int btn)
    {
        return btn switch
        {
            0 => spriteA,
            1 => spriteB,
            2 => spriteX,
            3 => spriteY,
            4 => spriteUp,
            5 => spriteDown,
            6 => spriteLeft,
            7 => spriteRight,
            _ => null
        };
    }

    void ResetCommand()
    {
        Debug.Log("コマンドをリセットします");
        if (useCustomCommand && presetCommand.Count > 0)
            UsePresetCommand();
        else
            GenerateRandomCommand();
    }

    void SpawnInfantry()
    {
        if (infantryPrefab == null)
        {
            Debug.LogWarning("歩兵プレハブが設定されていません！");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 offset = spawnOffset * i;
            Vector3 pos = transform.position + offset;
            Instantiate(infantryPrefab, pos, Quaternion.identity);
        }

        Debug.Log($"{spawnCount}体の歩兵をスポーンしました！");
    }
}
