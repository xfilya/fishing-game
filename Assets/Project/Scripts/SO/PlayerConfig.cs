using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/PlayerConfig")]
public sealed class PlayerConfig : ScriptableObject
{
    [field: Header("Movement")]
    [field: SerializeField] public float BaseSpeed { get; private set; } = 5f;
    [field: SerializeField] public float Gravity { get; private set; } = -20f;

    [field: Header("Camera")]
    [field: SerializeField] public float MouseSensitivity { get; private set; } = 0.1f;
    [field: SerializeField] public float StickSensitivity { get; private set; } = 180f;
    [field: SerializeField] public float CameraClampAngle { get; private set; } = 85f;   

    [field: Header("Jump")]
    [field: SerializeField] public float JumpHeight { get; private set; } = 1.5f;

    [field: Header("Sprint")]
    [field: SerializeField] public float SprintMultiplier { get; private set; } = 1.5f;
    [field: SerializeField] public float SprintFovOffset { get; private set; } = 5f;
    [field: SerializeField] public float SprintTransitionSpeed { get; private set; } = 0.15f;

}
