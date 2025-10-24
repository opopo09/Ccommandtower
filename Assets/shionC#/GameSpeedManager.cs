using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.InputSystem.Controls; // ← 【重要】この一行を追加しました

public class GameSpeedManager : MonoBehaviour
{
    [Header("速度設定")]
    [Tooltip("通常時のゲーム速度")]
    public float normalSpeed = 1.0f;
    [Tooltip("倍速時のゲーム速度")]
    public float fastSpeed = 1.5f;

    [Header("操作設定")]
    [Tooltip("速度切り替えに使用するコマンドボタン")]
    public CommandButton speedToggleButton = CommandButton.Y;

    [Header("UI設定 (省略可能)")]
    [Tooltip("現在の速度を表示するTextMeshProUGUIのテキスト")]
    public TextMeshProUGUI speedStatusText;

    private bool isFastMode = false;

    void Start()
    {
        Time.timeScale = normalSpeed;
        isFastMode = false;
        UpdateSpeedText();
    }

    void Update()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) return;

        InputControl buttonControl = GetInputControl(gamepad, speedToggleButton);

        // この行が正しく動作するようになります
        if (buttonControl != null && buttonControl is ButtonControl button && button.wasPressedThisFrame)
        {
            ToggleSpeed();
        }
    }

    public void ToggleSpeed()
    {
        isFastMode = !isFastMode;

        if (isFastMode)
        {
            Time.timeScale = fastSpeed;
        }
        else
        {
            Time.timeScale = normalSpeed;
        }

        UpdateSpeedText();
    }

    private void UpdateSpeedText()
    {
        if (speedStatusText != null)
        {
            if (isFastMode)
            {
                speedStatusText.text = $"Speed: x{fastSpeed}";
            }
            else
            {
                speedStatusText.text = $"Speed: x{normalSpeed}";
            }
        }
    }

    private InputControl GetInputControl(Gamepad gamepad, CommandButton button)
    {
        switch (button)
        {
            case CommandButton.A: return gamepad.buttonSouth;
            case CommandButton.B: return gamepad.buttonEast;
            case CommandButton.X: return gamepad.buttonWest;
            case CommandButton.Y: return gamepad.buttonNorth;
            case CommandButton.DPadUp: return gamepad.dpad.up;
            case CommandButton.DPadDown: return gamepad.dpad.down;
            case CommandButton.DPadLeft: return gamepad.dpad.left;
            case CommandButton.DPadRight: return gamepad.dpad.right;
            default: return null;
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1.0f;
    }
}