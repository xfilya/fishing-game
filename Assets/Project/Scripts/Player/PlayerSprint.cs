using UnityEngine;

public sealed class PlayerSprint : MonoBehaviour
{
    private IInputService _input;
    private PlayerConfig _config;
    private PlayerMovement _movement;
    private PlayerCamera _playerCamera;

    private bool _isSprinting;

    public void Initialize(
        IInputService inputService,
        PlayerConfig playerConfig,
        PlayerMovement playerMovement,
        PlayerCamera playerCamera)
    {
        _input = inputService;
        _config = playerConfig;
        _movement = playerMovement;
        _playerCamera = playerCamera;
    }

    private void Update()
    {
        if (_input == null)
            return;

        bool shouldSprint = _input.SprintHeld && _input.Movement.sqrMagnitude > 0.01f;

        if (shouldSprint == _isSprinting)
            return;

        _isSprinting = shouldSprint;
        _movement.SetSpeedMultiplier(_isSprinting ? _config.SprintMultiplier : 1f);
        _playerCamera.SetSprintFov(_isSprinting);
    }

    private void OnDisable()
    {
        _isSprinting = false;

        if (_movement != null)
            _movement.SetSpeedMultiplier(1f);

        if (_playerCamera != null)
            _playerCamera.SetSprintFov(false);
    }
}