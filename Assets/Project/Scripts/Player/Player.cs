using UnityEngine;
using VContainer;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerCamera _playerCamera;

    [Inject]
    public void Construct(
        IInputService inputService,
        PlayerConfig playerConfig)
    {
        _movement.Initialize(inputService, playerConfig);
        _playerCamera.Initialize(inputService, playerConfig);
    }
}