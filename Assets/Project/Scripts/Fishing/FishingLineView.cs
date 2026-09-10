using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public sealed class FishingLineView : MonoBehaviour
{
    [SerializeField, Min(2)] private int _segmentCount = 24;
    [SerializeField, Min(0.0001f)] private float _width = 0.004f;
    [SerializeField, Min(0f)] private float _baseSag = 0.05f;
    [SerializeField, Min(0f)] private float _slackSagMultiplier = 0.4f;
    [SerializeField, Min(0f)] private float _maxSag = 2f;
    [SerializeField] private Color _color = new(0.92f, 0.95f, 0.9f, 0.85f);

    private LineRenderer _lineRenderer;
    private Material _runtimeMaterial;
    private Transform _start;
    private Transform _end;
    private float _lineLength;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        if (_lineRenderer == null)
            _lineRenderer = gameObject.AddComponent<LineRenderer>();

        ConfigureLineRenderer();
    }

    public void Attach(Transform start, Transform end, float lineLength)
    {
        _start = start;
        _end = end;
        _lineLength = lineLength;
        _lineRenderer.enabled = true;
        UpdateLine();
    }

    public void SetLineLength(float lineLength)
    {
        _lineLength = lineLength;
    }

    public void Detach()
    {
        _start = null;
        _end = null;
        _lineRenderer.enabled = false;
    }

    private void LateUpdate()
    {
        UpdateLine();
    }

    private void ConfigureLineRenderer()
    {
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.positionCount = _segmentCount;
        _lineRenderer.startWidth = _width;
        _lineRenderer.endWidth = _width;
        _lineRenderer.startColor = _color;
        _lineRenderer.endColor = _color;
        _lineRenderer.numCapVertices = 2;
        _lineRenderer.textureMode = LineTextureMode.Stretch;
        _lineRenderer.enabled = false;

        if (_lineRenderer.sharedMaterial != null)
            return;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader == null)
            return;

        _runtimeMaterial = new Material(shader);
        _runtimeMaterial.color = _color;
        _lineRenderer.sharedMaterial = _runtimeMaterial;
    }

    private void UpdateLine()
    {
        if (_start == null || _end == null)
            return;

        Vector3 start = _start.position;
        Vector3 end = _end.position;
        float distance = Vector3.Distance(start, end);
        float sag = Mathf.Clamp(_baseSag + Mathf.Max(0f, _lineLength - distance) * _slackSagMultiplier, 0f, _maxSag);

        for (int i = 0; i < _segmentCount; i++)
        {
            float progress = i / (float)(_segmentCount - 1);
            Vector3 position = Vector3.Lerp(start, end, progress);
            position += Vector3.down * (4f * progress * (1f - progress) * sag);
            _lineRenderer.SetPosition(i, position);
        }
    }

    private void OnDestroy()
    {
        if (_runtimeMaterial != null)
            Destroy(_runtimeMaterial);
    }
}
