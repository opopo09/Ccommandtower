using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;

public static class GamepadInputHelper
{
    public static Dictionary<CommandButton, bool> GetPressedButtons(Gamepad gamepad)
    {
        var pressedButtons = new Dictionary<CommandButton, bool>();

        if (gamepad == null) return pressedButtons;

        pressedButtons[CommandButton.A] = gamepad.buttonSouth.wasPressedThisFrame;
        pressedButtons[CommandButton.B] = gamepad.buttonEast.wasPressedThisFrame;
        pressedButtons[CommandButton.X] = gamepad.buttonWest.wasPressedThisFrame;
        pressedButtons[CommandButton.Y] = gamepad.buttonNorth.wasPressedThisFrame;
        pressedButtons[CommandButton.DPadUp] = gamepad.dpad.up.wasPressedThisFrame;
        pressedButtons[CommandButton.DPadDown] = gamepad.dpad.down.wasPressedThisFrame;
        pressedButtons[CommandButton.DPadLeft] = gamepad.dpad.left.wasPressedThisFrame;
        pressedButtons[CommandButton.DPadRight] = gamepad.dpad.right.wasPressedThisFrame;

        return pressedButtons;
    }

    public static bool IsButtonPressed(Gamepad gamepad, CommandButton button)
    {
        return button switch
        {
            CommandButton.A => gamepad.buttonSouth.wasPressedThisFrame,
            CommandButton.B => gamepad.buttonEast.wasPressedThisFrame,
            CommandButton.X => gamepad.buttonWest.wasPressedThisFrame,
            CommandButton.Y => gamepad.buttonNorth.wasPressedThisFrame,
            CommandButton.DPadUp => gamepad.dpad.up.wasPressedThisFrame,
            CommandButton.DPadDown => gamepad.dpad.down.wasPressedThisFrame,
            CommandButton.DPadLeft => gamepad.dpad.left.wasPressedThisFrame,
            CommandButton.DPadRight => gamepad.dpad.right.wasPressedThisFrame,
            _ => false,
        };
    }
}
