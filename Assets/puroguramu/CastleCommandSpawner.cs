using UnityEngine;

using UnityEngine.UI;

using System.Collections.Generic;

public class CastleCommandSpawner : MonoBehaviour

{

    [Header("�X�|�[����������v���n�u")]

    public GameObject infantryPrefab;

    [Header("�X�|�[���ʒu�̃I�t�Z�b�g")]

    public Vector3 spawnOffset = new Vector3(1, 0, 0);

    [Header("�R�}���h�̗P�\���ԁi�b�j 0�Ŗ�����")]

    public float commandTimeout = 0f;

    [Header("�R�}���h�X�e�b�v���iInspector�ŕύX�\�j")]

    [Range(1, 10)]

    public int commandStepCount = 2;

    [Header("���݂̃R�}���h��\������Text")]

    public Text commandText;  // TextMeshProUGUI�ł�OK

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

        // �P�\���Ԑ؂�

        if (commandTimeout > 0 && Time.time - commandStartTime > commandTimeout)

        {

            Debug.Log("�R�}���h�^�C���A�E�g");

            ResetCommand();

            return;

        }

        // ���ɉ����ׂ��{�^���ԍ�

        int buttonToPress = currentCommand[currentStep];

        if (Input.GetKeyDown("joystick button " + buttonToPress))

        {

            Debug.Log($"Step {currentStep + 1}: joystick button {buttonToPress} ����");

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

        string text = "�R�}���h: ";

        foreach (int btn in currentCommand)

        {

            text += ButtonName(btn) + " �� ";

        }

        text = text.TrimEnd(' ', '��');

        commandText.text = text;

    }

    string ButtonName(int btn)

    {

        return btn switch

        {

            0 => "A(�Z)",

            1 => "B(�~)",

            2 => "X(��)",

            3 => "Y(��)",

            _ => "?"

        };

    }

    void ResetCommand()

    {

        Debug.Log("�R�}���h�����Z�b�g���܂�");

        GenerateRandomCommand();

    }

    void SpawnInfantry()

    {

        if (infantryPrefab == null)

        {

            Debug.LogWarning("�����v���n�u���ݒ肳��Ă��܂���I");

            return;

        }

        Vector3 spawnPosition = transform.position + spawnOffset;

        Instantiate(infantryPrefab, spawnPosition, Quaternion.identity);

        Debug.Log("�����X�|�[���I");

    }

}

