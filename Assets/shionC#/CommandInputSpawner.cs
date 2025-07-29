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

    [Header("コスト設定")]
    [SerializeField] private float spawnCost = 1.0f; // このコマンドで生成する際の消費ゲージコスト

    [Header("出現制限")]
    [SerializeField] private SpawnLimitChecker spawnLimitChecker;
    [SerializeField] private string[] spawnTags = { "Ally", "Support", "Minion" };
    [SerializeField] private int maxSpawnCount = 10;

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
        if (gamepad == null || commandSequence.Length == 0) return;

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
                        // コマンドが完成したらSpawnメソッドを呼び出す
                        Spawn();
                        ResetCommand();
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

    void ResetCommand()
    {
        currentIndex = 0;
        // 成功・失敗に関わらずコマンド入力が終わったら振動フラグをリセット
        vibrationTriggered = false;
    }

    void Spawn()
    {
        if (spawnPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("[CommandInputSpawner] spawnPrefab または spawnPoint が設定されていません。");
            return;
        }

        // --- 1. ゲージコストのチェック ---
        // GaugeManagerが存在し、かつゲージがコスト分だけあるか確認
        if (GaugeManager.Instance == null || !GaugeManager.Instance.UseGauge(spawnCost))
        {
            Debug.Log("ゲージが不足しているためスポーンできません。");
            PlaySE(mistakeSE);
            TriggerFailureVibration();
            return; // ゲージが足りないので処理を中断
        }

        // --- 2. 出現上限のチェック ---
        bool canSpawn = spawnLimitChecker != null
            ? spawnLimitChecker.CanSpawn()
            : SpawnLimitChecker.CanSpawnWithTags(spawnTags, maxSpawnCount);

        if (!canSpawn)
        {
            Debug.Log("出現上限に達しているためスポーンできません。");
            PlaySE(mistakeSE);
            TriggerFailureVibration();
            // 注意：上限で失敗した場合、先に消費したゲージを元に戻す
            GaugeManager.Instance.AddGauge(spawnCost);
            return;
        }

        // --- 3. 成功処理 ---
        // 全てのチェックを通過したら、成功音を鳴らして生成
        PlaySE(successSE);
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
            // ここに振動マネージャーの処理が入ります（元のコードのまま）
            // GamepadVibrationManager.PlayVibration(vibrationDuration, 0.6f, 0.6f, this);
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

    // （元のコードにない場合、以下のenum定義もスクリプト内または別のファイルに必要です）
    public enum CommandButton
    {
        A, B, X, Y, DPadUp, DPadDown, DPadLeft, DPadRight
    }
}