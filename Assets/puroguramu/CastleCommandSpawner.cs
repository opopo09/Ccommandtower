using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CastleCommandSpawner : MonoBehaviour
{
    [Header("スポーンする歩兵プレハブ")]
    public GameObject infantryPrefab;

    [Header("スポーン位置のオフセット")]
    public Vector3 spawnOffset = new Vector3(1, 0, 0);

    [Header("コマンドの猶予時間（秒） 0で無制限")]
    public float commandTimeout = 0f;

    [Header("コマンドステップ数（Inspectorで変更可能）")]
    [Range(1, 10)]
    public int commandStepCount = 2;

    [Header("コマンド画像を表示する親")]
    public Transform commandImagePanel;

    [Header("ボタン画像のプレハブ (Image付き)")]
    public GameObject commandImagePrefab;

    [Header("各ボタンのスプライト")]
    public Sprite spriteA;
    public Sprite spriteB;
    public Sprite spriteX;
    public Sprite spriteY;

    private List<int> currentCommand = new List<int>();
    private int currentStep = 0;
    private float commandStartTime = 0f;
    private List<GameObject> commandImages = new List<GameObject>();

    void Start()
    {
        GenerateRandomCommand();
    }

    void Update()
    {
        if (currentCommand.Count == 0) return;

        if (commandTimeout > 0 && Time.time - commandStartTime > commandTimeout)
        {
            Debug.Log("コマンドタイムアウト");
            ResetCommand();
            return;
        }

        int buttonToPress = currentCommand[currentStep];

        if (Input.GetKeyDown("joystick button " + buttonToPress))
        {
            Debug.Log($"Step {currentStep + 1}: joystick button {buttonToPress} 入力");
            HighlightCommandStep(currentStep);
            currentStep++;

            if (currentStep >= currentCommand.Count)
            {
                SpawnInfantry();
                GenerateRandomCommand();
            }
        }
    }

    void GenerateRandomCommand()
    {
        currentCommand.Clear();
        ClearCommandImages();

        for (int i = 0; i < commandStepCount; i++)
        {
            int randomButton = Random.Range(0, 4);
            currentCommand.Add(randomButton);
            AddCommandImage(randomButton);
        }

        currentStep = 0;
        commandStartTime = Time.time;
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
            img.color = Color.gray; // 入力済みのステップをグレーに
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
            _ => null
        };
    }

    void ResetCommand()
    {
        Debug.Log("コマンドをリセットします");
        GenerateRandomCommand();
    }

    void SpawnInfantry()
    {
        if (infantryPrefab == null)
        {
            Debug.LogWarning("歩兵プレハブが設定されていません！");
            return;
        }

        Vector3 spawnPosition = transform.position + spawnOffset;
        Instantiate(infantryPrefab, spawnPosition, Quaternion.identity);
        Debug.Log("歩兵スポーン！");
    }
}
