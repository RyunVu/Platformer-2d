using UnityEngine;

/// <summary>
/// Compatibility layer that bridges the new Input System with the existing PlayerMovement code
/// This allows the refactored code to work without changes
/// </summary>
public static class InputManager
{
    /// <summary>
    /// Movement input as Vector2 (WASD keys)
    /// </summary>
    public static Vector2 MoveInput
    {
        get
        {
            if (PlayerInput.Instance != null)
                return PlayerInput.Instance.moveInput;
            return Vector2.zero;
        }
    }

    /// <summary>
    /// True for one frame when jump button is pressed
    /// </summary>
    public static bool JumpWasPressed
    {
        get
        {
            if (PlayerInput.Instance != null)
                return PlayerInput.Instance.jumpWasPressed;
            return false;
        }
    }

    /// <summary>
    /// True for one frame when jump button is released
    /// </summary>
    public static bool JumpWasReleased
    {
        get
        {
            if (PlayerInput.Instance != null)
                return PlayerInput.Instance.jumpWasReleased;
            return false;
        }
    }

    /// <summary>
    /// True while jump button is held down
    /// </summary>
    public static bool JumpIsHeld
    {
        get
        {
            if (PlayerInput.Instance != null)
                return PlayerInput.Instance.jumpIsHeld;
            return false;
        }
    }

    /// <summary>
    /// True while run button is held down (Left Shift)
    /// </summary>
    public static bool RunIsHeld
    {
        get
        {
            if (PlayerInput.Instance != null)
                return PlayerInput.Instance.runIsHeld;
            return false;
        }
    }

    /// <summary>
    /// True for one frame when dash button is pressed (Q key)
    /// </summary>
    public static bool DashWasPressed
    {
        get
        {
            if (PlayerInput.Instance != null)
                return PlayerInput.Instance.dashWasPressed;
            return false;
        }
    }

    /// <summary>
    /// True for one frame when interact button is pressed (E key)
    /// </summary>
    public static bool InteractWasPressed
    {
        get
        {
            if (PlayerInput.Instance != null)
                return PlayerInput.Instance.interactWasPressed;
            return false;
        }
    }

    // Additional helper methods for convenience

    /// <summary>
    /// Returns horizontal input as raw value (-1, 0, 1)
    /// </summary>
    public static float GetHorizontalRaw()
    {
        if (PlayerInput.Instance != null)
            return PlayerInput.Instance.GetHorizontalInput();
        return 0f;
    }

    /// <summary>
    /// Returns vertical input as raw value (-1, 0, 1)
    /// </summary>
    public static float GetVerticalRaw()
    {
        if (PlayerInput.Instance != null)
            return PlayerInput.Instance.GetVerticalInput();
        return 0f;
    }

    /// <summary>
    /// Check if player is giving any movement input
    /// </summary>
    public static bool IsMoving()
    {
        if (PlayerInput.Instance != null)
            return PlayerInput.Instance.IsMovingHorizontally();
        return false;
    }
}