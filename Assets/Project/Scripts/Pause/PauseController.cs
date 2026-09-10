using UnityEngine;
using VContainer;

public sealed class PauseController : MonoBehaviour
{
    private IInputService _inputService;
    private UIService _uiService;
    private Player _player;


    private bool _isPaused = false;

    [Inject]
    public void Construct(IInputService inputService, UIService uiService, Player player)
    {
        _inputService = inputService;
        _uiService = uiService;
        _player = player;
    }

    private void OnEnable()
    {
        _isPaused = false;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        SetPauseState(false);
    }

    private void OnDisable()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        _inputService?.SetGameplayEnabled(true);
    }

    private void Update()
    {
        if (_inputService == null)
            return;

        if (_inputService.EscapePressedThisFrame)
        {
            SetPauseState(!_isPaused);
        }
    }

    private void SetPauseState(bool isPaused)
    {
        _isPaused = isPaused;
        _inputService.SetGameplayEnabled(!isPaused);
        _uiService.OpenMenu(isPaused);
        _player.EnableCamera(!isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }
}
