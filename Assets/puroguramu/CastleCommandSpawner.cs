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

    [Header("�R�}���h�摜��\������e")]
    public Transform commandImagePanel;

    [Header("�{�^���摜�̃v���n�u (Image�t��)")]
    public GameObject commandImagePrefab;

    [Header("�e�{�^���̃X�v���C�g")]
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
            Debug.Log("�R�}���h�^�C���A�E�g");
            ResetCommand();
            return;
        }

        int buttonToPress = currentCommand[currentStep];

        if (Input.GetKeyDown("joystick button " + buttonToPress))
        {
            Debug.Log($"Step {currentStep + 1}: joystick button {buttonToPress} ����");
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
            img.color = Color.gray; // ���͍ς݂̃X�e�b�v���O���[��
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
