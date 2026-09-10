using UnityEngine;

public interface IInputService
{
    Vector2 Movement { get; }
    Vector2 Look { get; }

    bool IsMouse { get; }

    bool JumpPressedThisFrame { get; }
    bool SprintHeld { get; }
    bool EscapePressedThisFrame { get; }
    bool PrimaryActionPressedThisFrame { get; }
    void SetGameplayEnabled(bool isEnabled);
}
