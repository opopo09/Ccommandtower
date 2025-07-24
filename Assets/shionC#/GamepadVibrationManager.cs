using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public static class GamepadVibrationManager
{
    private static Coroutine vibrationCoroutine;
    private static MonoBehaviour coroutineRunner;

    public static bool IsVibrating { get; private set; } = false;

    /// <summary>
    /// 振動を開始します（失敗時など限定的に使用）
    /// </summary>
    public static void PlayVibration(float duration, float lowFreq, float highFreq, MonoBehaviour caller)
    {
        // 現在振動中、または Gamepad が未接続なら無視
        if (IsVibrating || Gamepad.current == null || caller == null)
            return;

        coroutineRunner = caller;
        IsVibrating = true;

        Gamepad.current.SetMotorSpeeds(lowFreq, highFreq);

        // 既存コルーチンがあれば停止
        if (vibrationCoroutine != null)
        {
            coroutineRunner.StopCoroutine(vibrationCoroutine);
            vibrationCoroutine = null;
        }

        vibrationCoroutine = coroutineRunner.StartCoroutine(StopVibrationAfterDelay(duration));
    }

    private static IEnumerator StopVibrationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopVibration();
    }

    /// <summary>
    /// 振動を強制的に停止します
    /// </summary>
    public static void StopVibration()
    {
        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(0f, 0f);

        IsVibrating = false;

        if (vibrationCoroutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(vibrationCoroutine);
            vibrationCoroutine = null;
        }
    }

    /// <summary>
    /// 状態を初期化（シーン切り替え・再起動時など）
    /// </summary>
    public static void Reset()
    {
        StopVibration();
        coroutineRunner = null;
    }
}
