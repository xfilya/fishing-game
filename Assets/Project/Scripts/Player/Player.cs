using UnityEngine;
using VContainer;

public sealed class Player : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;

    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerCamera _playerCamera;
    [SerializeField] private PlayerJump _jump;
    [SerializeField] private PlayerSprint _sprint;

    private IInputService _inputService;
    private UIService _uiService;

    private bool _respawnRequested;

    private bool _isMenuOpen = false;

    [Inject]
    public void Construct(
        IInputService inputService,
        PlayerConfig playerConfig,
        UIService uiService)
    {
        _movement.Initialize(inputService, playerConfig);
        _playerCamera.Initialize(inputService, playerConfig);
        _jump.Initialize(inputService, playerConfig, _movement);
        _sprint.Initialize(inputService, playerConfig, _movement, _playerCamera);

        _inputService = inputService;
        _uiService = uiService;
    }

    private void Start()
    {
        MoveToSpawnPoint();
    }

    private void Update()
    {
        if (_inputService == null)
            return;

        if (_inputService.EscapePressedThisFrame)
        {
            _isMenuOpen = !_isMenuOpen;
            EscapePressed(_isMenuOpen);
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<WaterFlag>(out _))
            _respawnRequested = true;
    }

    private void EscapePressed(bool isMenuOpen)
    {
        _playerCamera.enabled = !isMenuOpen;
        _uiService.OpenMenu(isMenuOpen);
    }
    
}