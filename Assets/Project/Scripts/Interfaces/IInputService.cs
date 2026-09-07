using UnityEngine;

public interface IInputService
{
    Vector2 Movement { get; }
    Vector2 Look { get; }

    bool IsMouse { get; }
}
