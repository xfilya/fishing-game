using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IInitializePotentialDragHandler
{
    [SerializeField] private RectTransform _base;
    [SerializeField] private RectTransform _handle;
    [SerializeField, Range(0f, 0.5f)] private float _deadZone = 0.08f;

    private MobileInputService _input;
    private int _pointerId = int.MinValue;

    public void Initialize(MobileInputService input) => _input = input;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pointerId != int.MinValue)
            return;

        _pointerId = eventData.pointerId;
        UpdateStick(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_pointerId == eventData.pointerId)
            UpdateStick(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_pointerId == eventData.pointerId)
            ResetInput();
    }

    public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;
    private void OnDisable() => ResetInput();

    public void ResetInput()
    {
        _pointerId = int.MinValue;
        _input?.SetMovement(Vector2.zero);
        if (_handle != null)
            _handle.anchoredPosition = Vector2.zero;
    }

    private void UpdateStick(PointerEventData eventData)
    {
        if (_input == null || !RectTransformUtility.ScreenPointToLocalPointInRectangle(_base, eventData.position, eventData.pressEventCamera, out Vector2 position))
            return;

        float radius = Mathf.Max(1f, (_base.rect.width - _handle.rect.width) * 0.5f);
        Vector2 direction = Vector2.ClampMagnitude(position / radius, 1f);
        _handle.anchoredPosition = direction * radius;
        float magnitude = direction.magnitude;
        _input.SetMovement(magnitude <= _deadZone ? Vector2.zero : direction.normalized * ((magnitude - _deadZone) / (1f - _deadZone)));
    }
}
