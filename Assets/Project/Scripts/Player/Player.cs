using UnityEngine;
using VContainer;

public sealed class Player : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;

    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerCamera _playerCamera;
    [SerializeField] private PlayerJump _jump;
    [SerializeField] private PlayerSprint _sprint;

    private bool _respawnRequested;

    [Inject]
    public void Construct(
        IInputService inputService,
        PlayerConfig playerConfig
        )
    {
        _movement.Initialize(inputService, playerConfig);
        _playerCamera.Initialize(inputService, playerConfig);
        _jump.Initialize(inputService, playerConfig, _movement);
        _sprint.Initialize(inputService, playerConfig, _movement, _playerCamera);
    }

    private void Start()
    {
        MoveToSpawnPoint();
    }


    private void LateUpdate()
    {
        if (!_respawnRequested)
            return;

        _respawnRequested = false;
        _movement.Teleport(_spawnPoint);
    }

    public void MoveToSpawnPoint()
    {       
        transform.SetPositionAndRotation(_spawnPoint.position, _spawnPoint.rotation);
    }

    public void EnableCamera(bool isEnabled)
    {
        _playerCamera.enabled = isEnabled;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<WaterFlag>(out _))
            _respawnRequested = true;
    }
    
}