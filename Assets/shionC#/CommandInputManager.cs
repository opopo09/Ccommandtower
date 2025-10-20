using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.InputSystem.LowLevel;

public class CommandInputManager : MonoBehaviour
{
    public static CommandInputManager instance;

    // どのボタンが押されたかを、他のスクリプトに通知するためのイベント
    public event Action<GamepadButton> OnButtonPressed;

    private Gamepad gamepad;
    private GamepadButton[] checkButtons = new GamepadButton[]
    {
        GamepadButton.South, GamepadButton.East, GamepadButton.West, GamepadButton.North,
        GamepadButton.DpadUp, GamepadButton.DpadDown, GamepadButton.DpadLeft, GamepadButton.DpadRight
    };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいで存在させる場合
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        gamepad ??= Gamepad.current;
        if (gamepad == null) return;

        // 監視対象のボタンが押されたかチェック
        foreach (var button in checkButtons)
        {
            if (gamepad[button].wasPressedThisFrame)
            {
                // ボタンが押されたことを、登録されている全てのスクリプトに通知する
                OnButtonPressed?.Invoke(button);
                // 1フレームに複数のコマンドスクリプトが反応しないように、一度通知したらループを抜ける
                break;
            }
        }
    }
}