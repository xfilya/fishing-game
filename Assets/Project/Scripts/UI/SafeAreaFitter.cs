using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class SafeAreaFitter : MonoBehaviour
{
    private RectTransform _rect;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;

    private void Awake() => _rect = (RectTransform)transform;
    private void OnEnable() => Refresh();
    private void Update() => Refresh();

    private void Refresh()
    {
        if (_rect == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safeArea = Screen.safeArea;
        Vector2Int screenSize = new(Screen.width, Screen.height);
        if (_lastSafeArea == safeArea && _lastScreenSize == screenSize)
            return;

        _lastSafeArea = safeArea;
        _lastScreenSize = screenSize;
        _rect.anchorMin = new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height);
        _rect.anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
        _rect.offsetMin = Vector2.zero;
        _rect.offsetMax = Vector2.zero;
    }
}
