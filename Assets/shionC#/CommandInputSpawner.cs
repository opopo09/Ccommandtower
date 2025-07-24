using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CommandInputSpawner : MonoBehaviour
{
    [Header("コマンド列")]
    public CommandButton[] commandSequence;

    [Header("スポーン設定")]
    public GameObject spawnPrefab;
    public Transform spawnPoint;

    [Header("音声")]
    public AudioClip buttonSE;
    public AudioClip successSE;
    public AudioClip mistakeSE;
    public AudioSource audioSource;

    [Header("バイブレーション時間")]
    public float vibrationDuration = 0.2f;

    private int currentIndex = 0;
    private Gamepad gamepad;
    private bool vibrationTriggered = false;

    void Update()
    {
        gamepad ??= Gamepad.current;
        if (gamepad == null) return;

        foreach (var control in gamepad.allControls)
        {
            if (control is ButtonControl button && button.wasPressedThisFrame)
            {
                if (button == gamepad.leftStickButton || button == gamepad.rightStickButton)
                    continue;

                PlaySE(buttonSE);

                string inputPath = control.path.Replace(gamepad.path + "/", "");
                string expectedPath = GetControlPath(commandSequence[currentIndex]);

                if (inputPath == expectedPath)
                {
                    currentIndex++;

                    if (currentIndex >= commandSequence.Length)
                    {
                        PlaySE(successSE);
                        Spawn();
                        ResetCommand();
                        vibrationTriggered = false;
                    }
                }
                else
                {
                    PlaySE(mistakeSE);
                    TriggerFailureVibration();
                    ResetCommand();
                }

                break;
            }
        }
    }

    void ResetCommand() => currentIndex = 0;

    void Spawn()
    {
        if (spawnPrefab != null && spawnPoint != null)
            Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation);
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

    string GetControlPath(CommandButton button) => button switch
    {
        CommandButton.A => "buttonSouth",
        CommandButton.B => "buttonEast",
        CommandButton.X => "buttonWest",
        CommandButton.Y => "buttonNorth",
        CommandButton.DPadUp => "dpad/up",
        CommandButton.DPadDown => "dpad/down",
        CommandButton.DPadLeft => "dpad/left",
        CommandButton.DPadRight => "dpad/right",
        _ => ""
    };
}
