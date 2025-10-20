using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// --- ダミー定義を完全に削除しました ---

public class CommandInputSpawner : MonoBehaviour
{
    [Header("コマンド列")]
    public CommandButton[] commandSequence;

    [Header("スポーン設定")]
    public GameObject spawnPrefab;
    public Transform spawnPoint;

    [Header("出現制限チェック（必須）")]
    [SerializeField] private SpawnLimitChecker spawnLimitChecker;

    [Header("音声")]
    public AudioClip successSE;
    public AudioClip mistakeSE;
    public AudioSource audioSource;

    [Header("バイブレーション時間")]
    public float vibrationDuration = 0.2f;

    private int currentIndex = 0;

    void Start()
    {
        if (CommandInputManager.instance != null)
        {
            CommandInputManager.instance.OnButtonPressed += HandleButtonPress;
        }
        else
        {
            Debug.LogError("CommandInputManagerがシーンに存在しません！", this.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (CommandInputManager.instance != null)
        {
            CommandInputManager.instance.OnButtonPressed -= HandleButtonPress;
        }
    }

    private void HandleButtonPress(GamepadButton pressedButton)
    {
        if (commandSequence == null || commandSequence.Length == 0 || currentIndex >= commandSequence.Length) return;

        GamepadButton expectedButton = GetGamepadButton(commandSequence[currentIndex]);

        if (pressedButton == expectedButton)
        {
            currentIndex++;
            if (currentIndex >= commandSequence.Length)
            {
                PlaySE(successSE);
                TrySpawn();
                ResetCommand();
            }
        }
        else
        {
            PlaySE(mistakeSE);
            TriggerFailureVibration();
            ResetCommand();
        }
    }

    void ResetCommand()
    {
        currentIndex = 0;
    }

    void TrySpawn()
    {
        if (spawnPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("[CommandInputSpawner] spawnPrefab または spawnPoint が設定されていません。");
            return;
        }

        if (spawnLimitChecker != null && !spawnLimitChecker.CanSpawn())
        {
            Debug.Log("出現上限に達しているためスポーンできません。");
            PlaySE(mistakeSE);
            TriggerFailureVibration();
        }
        else
        {
            Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    void PlaySE(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    void TriggerFailureVibration()
    {
        GamepadVibrationManager.PlayVibration(vibrationDuration, 0.6f, 0.6f, this);
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
        _ => default
    };
}