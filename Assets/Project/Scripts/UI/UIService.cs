using UnityEngine;

public class UIService : MonoBehaviour
{
    [SerializeField] private GameObject _menu;

    private bool _isPauseMenuOpen;

    public bool IsMainMenuOpen { get; private set; }
    public bool IsMenuOpen => _isPauseMenuOpen || IsMainMenuOpen;

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

    private void RefreshCursor()
    {
        Cursor.visible = IsMenuOpen;
        Cursor.lockState = IsMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
