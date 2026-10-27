using UnityEngine;
using UnityEngine.EventSystems;

public enum MobileAction
{
    Primary,
    Jump,
    Sprint,
    Interact,
    Pause
}

[RequireComponent(typeof(UnityEngine.UI.Button))]
public sealed class MobileActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private MobileAction _action;

    private MobileInputService _input;
    private UnityEngine.UI.Button _button;
    private int _pointerId = int.MinValue;

    public UnityEngine.UI.Button Button => _button;

    public void Initialize(MobileInputService input)
    {
        _input = input;
        _button = GetComponent<UnityEngine.UI.Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_input == null || !_button.IsInteractable() || _pointerId != int.MinValue)
            return;

        _pointerId = eventData.pointerId;
        switch (_action)
        {
            case MobileAction.Primary: _input.PressPrimary(); break;
            case MobileAction.Jump: _input.PressJump(); break;
            case MobileAction.Sprint: _input.ToggleSprint(); break;
            case MobileAction.Interact: _input.PressInteract(); break;
            case MobileAction.Pause: _input.PressPause(); break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_pointerId == eventData.pointerId)
            _pointerId = int.MinValue;
    }

    public void ResetInput() => _pointerId = int.MinValue;
    private void OnDisable() => ResetInput();
}
