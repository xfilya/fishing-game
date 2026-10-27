using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MobileLookArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IInitializePotentialDragHandler
{
    private MobileInputService _input;
    private int _pointerId = int.MinValue;
    private Vector2 _lastPosition;

    public void Initialize(MobileInputService input) => _input = input;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_pointerId != int.MinValue)
            return;

        _pointerId = eventData.pointerId;
        _lastPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_pointerId != eventData.pointerId || _input == null)
            return;

        float screenSize = Mathf.Max(1f, Mathf.Min(Screen.width, Screen.height));
        _input.AddLook((eventData.position - _lastPosition) / screenSize);
        _lastPosition = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_pointerId == eventData.pointerId)
            ResetInput();
    }

    public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;
    public void ResetInput() => _pointerId = int.MinValue;
    private void OnDisable() => ResetInput();
}
