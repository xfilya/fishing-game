using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public sealed class ResponsivePanelScaler : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _screenMargin = 32f;

    private RectTransform _rectTransform;
    private RectTransform _canvasRectTransform;

    private void OnEnable()
    {
        _rectTransform = (RectTransform)transform;
        Canvas canvas = GetComponentInParent<Canvas>();
        _canvasRectTransform = canvas != null ? (RectTransform)canvas.transform : null;
        RefreshScale();
    }

    private void OnRectTransformDimensionsChange()
    {
        RefreshScale();
    }

    private void RefreshScale()
    {
        if (_rectTransform == null || _canvasRectTransform == null || _rectTransform.sizeDelta.x <= 0f || _rectTransform.sizeDelta.y <= 0f)
            return;

        float widthScale = Mathf.Max(0.1f, (_canvasRectTransform.rect.width - _screenMargin * 2f) / _rectTransform.sizeDelta.x);
        float heightScale = Mathf.Max(0.1f, (_canvasRectTransform.rect.height - _screenMargin * 2f) / _rectTransform.sizeDelta.y);
        float scale = Mathf.Min(1f, widthScale, heightScale);
        _rectTransform.localScale = Vector3.one * scale;
    }
}
