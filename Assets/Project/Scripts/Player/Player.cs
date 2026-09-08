using UnityEngine;
using VContainer;

public sealed class Player : MonoBehaviour
{
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerCamera _playerCamera;
    [SerializeField] private PlayerJump _jump;
    [SerializeField] private PlayerSprint _sprint;

    [Inject]
    public void Construct(
        IInputService inputService,
        PlayerConfig playerConfig)
    {
        _movement.Initialize(inputService, playerConfig);
        _playerCamera.Initialize(inputService, playerConfig);
        _jump.Initialize(inputService, playerConfig, _movement);
        _sprint.Initialize(inputService, playerConfig, _movement, _playerCamera);
    }
}