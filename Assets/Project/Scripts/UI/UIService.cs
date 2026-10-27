using UnityEngine;
using VContainer;

public class UIService : MonoBehaviour
{
    [SerializeField] private GameObject _menu;

    private bool _isPauseMenuOpen;
    private IInputService _input;

    public bool IsMainMenuOpen { get; private set; }
    public bool IsMenuOpen => _isPauseMenuOpen || IsMainMenuOpen;

    [Inject]
    public void Construct(IInputService input) => _input = input;

    public void OpenMenu(bool isOpen)
    {
        _isPauseMenuOpen = isOpen;
        RefreshCursor();

        if (_menu != null)
            _menu.SetActive(isOpen);
    }

    public void SetMainMenuOpen(bool isOpen)
    {
        IsMainMenuOpen = isOpen;
        RefreshCursor();
    }

    public void RefreshCursor()
    {
        bool freeCursor = IsMenuOpen || _input.IsTouch;
        Cursor.visible = freeCursor;
        Cursor.lockState = freeCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
