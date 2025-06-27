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

    [Header("現在のコマンドを表示するText")]

    public Text commandText;  // TextMeshProUGUIでもOK

    private List<int> currentCommand = new List<int>();

    private int currentStep = 0;

    private float commandStartTime = 0f;

    void Start()

    {

        GenerateRandomCommand();

    }

    void Update()

    {

        if (currentCommand.Count == 0) return;

        // 猶予時間切れ

        if (commandTimeout > 0 && Time.time - commandStartTime > commandTimeout)

        {

            Debug.Log("コマンドタイムアウト");

            ResetCommand();

            return;

        }

        // 次に押すべきボタン番号

        int buttonToPress = currentCommand[currentStep];

        if (Input.GetKeyDown("joystick button " + buttonToPress))

        {

            Debug.Log($"Step {currentStep + 1}: joystick button {buttonToPress} 入力");

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

        for (int i = 0; i < commandStepCount; i++)

        {

            int randomButton = Random.Range(0, 4); // 0:A, 1:B, 2:X, 3:Y

            currentCommand.Add(randomButton);

        }

        currentStep = 0;

        commandStartTime = Time.time;

        UpdateCommandText();

    }

    void UpdateCommandText()

    {

        if (commandText == null) return;

        string text = "コマンド: ";

        foreach (int btn in currentCommand)

        {

            text += ButtonName(btn) + " → ";

        }

        text = text.TrimEnd(' ', '→');

        commandText.text = text;

    }

    string ButtonName(int btn)

    {

        return btn switch

        {

            0 => "A(〇)",

            1 => "B(×)",

            2 => "X(□)",

            3 => "Y(△)",

            _ => "?"

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

