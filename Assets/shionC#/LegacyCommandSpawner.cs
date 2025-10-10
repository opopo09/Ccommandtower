using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class LegacyCommandSpawner : MonoBehaviour
{
    [Header("コマンド列")]
    public CommandButton[] commandSequence;

    [Header("プレビュー画像")]
    public Image previewImage;

    [Header("スポーン設定")]
    public GameObject spawnPrefab;
    public float spawnDistanceFromCamera = 3f;

    [Header("音声")]
    public AudioSource audioSource;
    public AudioClip successSE;
    public AudioClip mistakeSE;

    [Header("バイブレーション時間")]
    public float vibrationDuration = 0.3f;

    [Header("出現制限チェック（省略可能）")]
    [SerializeField] private SpawnLimitChecker spawnLimitChecker;

    // fallback 用（spawnLimitChecker が設定されていない場合のみ使う）
    [SerializeField] private string[] spawnTags = { "Ally", "Support", "Minion" };
    [SerializeField] private int maxSpawnCount = 10;

    private int currentIndex = 0;
    private bool isPreviewShown = false;
    private Camera mainCamera;
    private bool triggerPressedLastFrame = false;
    private bool vibrationTriggered = false;

    public bool IsLegacyFinished { get; private set; } = false;

    void Start()
    {
        mainCamera = Camera.main;
        previewImage?.gameObject.SetActive(false);
        IsLegacyFinished = false;
    }

    void Update()
    {
        if (IsLegacyFinished) return;

        var gamepad = Gamepad.current;
        if (gamepad == null) return;

        if (!isPreviewShown && currentIndex < commandSequence.Length)
        {
            var expected = GetGamepadButton(commandSequence[currentIndex]);

            if (IsButtonPressed(gamepad, expected))
            {
                currentIndex++;
                PlaySE(successSE);
                vibrationTriggered = false;

                if (currentIndex >= commandSequence.Length)
                {
                    isPreviewShown = true;
                    previewImage?.gameObject.SetActive(true);
                }
            }
            else if (AnyOtherValidButtonPressed(gamepad, expected))
            {
                TriggerFailureVibration();
                ResetCommand();
            }
        }
        else if (isPreviewShown)
        {
            bool triggerPressedNow = gamepad.rightTrigger.ReadValue() > 0.5f;

            if (triggerPressedNow && !triggerPressedLastFrame)
            {
                TrySpawnAtCameraFront();
                previewImage?.gameObject.SetActive(false);
                ResetCommand();
            }

            triggerPressedLastFrame = triggerPressedNow;
        }
    }

    void ResetCommand()
    {
        currentIndex = 0;
        isPreviewShown = false;
        previewImage?.gameObject.SetActive(false);
        triggerPressedLastFrame = false;
    }

    void TrySpawnAtCameraFront()
    {
        if (spawnPrefab == null || mainCamera == null) return;

        bool canSpawn = spawnLimitChecker != null
            ? spawnLimitChecker.CanSpawn()
            : SpawnLimitChecker.CanSpawnWithTags(spawnTags, maxSpawnCount);

        if (!canSpawn)
        {
            Debug.Log("出現上限に達しているためスポーンできません。");
            PlaySE(mistakeSE);
            TriggerFailureVibration();
            return;
        }

        Vector3 pos = mainCamera.transform.position + mainCamera.transform.forward * spawnDistanceFromCamera;
        Quaternion rot = Quaternion.LookRotation(mainCamera.transform.forward);
        Instantiate(spawnPrefab, pos, rot);
    }

    void PlaySE(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    void TriggerFailureVibration()
    {
        if (!vibrationTriggered)
        {
            GamepadVibrationManager.PlayVibration(vibrationDuration, 0.6f, 0.6f, this);
            vibrationTriggered = true;
        }
    }

    bool IsButtonPressed(Gamepad gamepad, GamepadButton btn) => gamepad[btn].wasPressedThisFrame;

    bool AnyOtherValidButtonPressed(Gamepad gamepad, GamepadButton expected)
    {
        GamepadButton[] validButtons =
        {
            GamepadButton.South, GamepadButton.East, GamepadButton.West, GamepadButton.North,
            GamepadButton.DpadUp, GamepadButton.DpadDown, GamepadButton.DpadLeft, GamepadButton.DpadRight
        };

        foreach (var btn in validButtons)
            if (btn != expected && IsButtonPressed(gamepad, btn))
                return true;

        return false;
    }

    GamepadButton GetGamepadButton(CommandButton btn) => btn switch
    {
        CommandButton.A => GamepadButton.South,
        CommandButton.B => GamepadButton.East,
        CommandButton.X => GamepadButton.West,
        CommandButton.Y => GamepadButton.North,
        CommandButton.DPadUp => GamepadButton.DpadUp,
        CommandButton.DPadDown => GamepadButton.DpadDown,
        CommandButton.DPadLeft => GamepadButton.DpadLeft,
        CommandButton.DPadRight => GamepadButton.DpadRight,
        _ => GamepadButton.Select
    };
}
