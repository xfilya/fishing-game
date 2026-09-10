using UnityEngine;

public class UIService : MonoBehaviour
{
    [SerializeField] private GameObject _menu;


    public void OpenMenu(bool isOpen)
    {
        Cursor.visible = isOpen;
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;

        if (_menu != null)
            _menu.SetActive(isOpen);
    }
}
