using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem.LowLevel;

public class LegacyCommandSpawner : MonoBehaviour
{
    [Header("コマンド列")]
    public CommandButton[] commandSequence;

    [Header("プレビュー画像")]
    public Image previewImage;

    [Header("スポーン設定")]
    public GameObject spawnPrefab;
    public float spawnDistanceFromCamera = 3f;

    [Header("失敗時の演出")]
    [Tooltip("失敗時にプレビューが赤く点滅する時間")]
    public float failureFlashDuration = 0.4f;

    [Header("音声")]
    public AudioSource audioSource;
    public AudioClip successSE;
    public AudioClip mistakeSE;

    [Header("バイブレーション時間")]
    public float vibrationDuration = 0.3f;

    [Header("出現制限チェック（必須）")]
    [SerializeField] private SpawnLimitChecker spawnLimitChecker;

    private int currentIndex = 0;
    private bool isPreviewShown = false;
    private Camera mainCamera;
    private bool triggerPressedLastFrame = false;
    private Color originalPreviewColor;
    private bool isHandlingFailure = false;

    void Start()
    {
        mainCamera = Camera.main;
        if (previewImage != null) { originalPreviewColor = previewImage.color; previewImage.gameObject.SetActive(false); }
        if (CommandInputManager.instance != null) { CommandInputManager.instance.OnButtonPressed += HandleButtonPress; }
        else { Debug.LogError("CommandInputManagerがシーンに存在しません！", this.gameObject); }
    }

    private void OnDestroy()
    {
        if (CommandInputManager.instance != null) { CommandInputManager.instance.OnButtonPressed -= HandleButtonPress; }
    }

    void Update()
    {
        if (isPreviewShown && !isHandlingFailure)
        {
            var gamepad = Gamepad.current;
            if (gamepad == null) return;
            bool triggerPressedNow = gamepad.rightTrigger.ReadValue() > 0.5f;
            if (triggerPressedNow && !triggerPressedLastFrame) { TrySpawnAtCameraFront(); }
            triggerPressedLastFrame = triggerPressedNow;
        }
    }

    private void HandleButtonPress(GamepadButton pressedButton)
    {
        if (isPreviewShown || currentIndex >= commandSequence.Length) return;
        GamepadButton expectedButton = GetGamepadButton(commandSequence[currentIndex]);
        if (pressedButton == expectedButton)
        {
            currentIndex++;
            PlaySE(successSE);
            if (currentIndex >= commandSequence.Length) { isPreviewShown = true; if (previewImage != null) previewImage.gameObject.SetActive(true); }
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
        isPreviewShown = false;
        if (previewImage != null) { previewImage.gameObject.SetActive(false); previewImage.color = originalPreviewColor; }
        triggerPressedLastFrame = false;
    }

    void TrySpawnAtCameraFront()
    {
        if (spawnPrefab == null || mainCamera == null) { ResetCommand(); return; }

        // ▼▼▼▼▼【ここからが新しいAIの核心部分です】▼▼▼▼▼

        // 1. 【座標計算の修正】カメラのX,Y座標を使い、Z=0の平面にスポーン位置を計算
        Vector3 spawnPos = mainCamera.transform.position;
        spawnPos.z = 0f; // 2D平面に強制
        spawnPos += mainCamera.transform.up * spawnDistanceFromCamera; // カメラの上方向にオフセット（ゲーム画面に合わせて調整）

        // 2. 障害物チェック（AIの頭脳に問い合わせる）
        if (!EnemyAI.IsPositionWalkable(spawnPos))
        {
            Debug.Log("出現場所が障害物でブロックされています。");
            StartCoroutine(FailureRoutine());
            return;
        }

        if (spawnLimitChecker != null && !spawnLimitChecker.CanSpawn())
        {
            Debug.Log("出現上限に達しているためスポーンできません。");
            StartCoroutine(FailureRoutine());
            return;
        }

        // 3. 【初期化処理の追加】スポーンと同時に、生命を吹き込む
        GameObject spawnedObj = Instantiate(spawnPrefab, spawnPos, Quaternion.identity); // 回転は不要なのでIdentityに

        // WaveManagerと同様の初期化処理を実行
        Enemy enemyScript = spawnedObj.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            // 基本ステータス（倍率1.0）で初期化
            enemyScript.Initialize(1f, 1f, 1f);
        }
        else
        {
            BoarEnemy boar = spawnedObj.GetComponent<BoarEnemy>();
            if (boar != null)
            {
                boar.Initialize(1f, 1f, 1f);
            }
        }

        // ▲▲▲▲▲【ここまで】▲▲▲▲▲

        ResetCommand();
    }

    IEnumerator FailureRoutine()
    {
        isHandlingFailure = true;
        PlaySE(mistakeSE);
        TriggerFailureVibration();
        if (previewImage != null) { previewImage.color = Color.red; }
        yield return new WaitForSeconds(failureFlashDuration);
        ResetCommand();
        isHandlingFailure = false;
    }

    void PlaySE(AudioClip clip) { if (audioSource != null && clip != null) audioSource.PlayOneShot(clip); }
    void TriggerFailureVibration() { GamepadVibrationManager.PlayVibration(vibrationDuration, 0.6f, 0.6f, this); }
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