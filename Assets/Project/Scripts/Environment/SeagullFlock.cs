using UnityEngine;

public sealed class SeagullFlock : MonoBehaviour
{
    [SerializeField] private SeagullFlight _seagullPrefab;
    [SerializeField] private Camera _viewCamera;
    [SerializeField] private AudioService _audioService;
    [SerializeField, Min(1)] private int _count = 8;
    [SerializeField, Min(1f)] private float _radius = 40f;
    [SerializeField, Min(1f)] private float _minimumHeight = 13f;
    [SerializeField, Min(1f)] private float _maximumHeight = 21f;
    [SerializeField, Min(0.1f)] private float _minimumSpeed = 2.8f;
    [SerializeField, Min(0.1f)] private float _maximumSpeed = 6.2f;
    [SerializeField, Min(0.1f)] private float _minimumCallInterval = 8f;
    [SerializeField, Min(0.1f)] private float _maximumCallInterval = 17f;

    private SeagullFlight[] _seagulls;
    private float _nextCallTime;

    private void Start()
    {
        if (_seagullPrefab == null)
        {
            Debug.LogError("[SeagullFlock] Seagull prefab is not assigned", this);
            return;
        }

        Camera viewCamera = _viewCamera != null ? _viewCamera : Camera.main;
        _seagulls = new SeagullFlight[_count];

        for (int index = 0; index < _count; index++)
        {
            Vector3 position = GetStartPosition(index, viewCamera);
            SeagullFlight seagull = Instantiate(_seagullPrefab, position, Quaternion.identity, transform);
            seagull.name = $"Seagull {index + 1}";
            seagull.transform.localScale = Vector3.one * Random.Range(0.43f, 0.55f);
            seagull.Initialize(viewCamera != null ? viewCamera.transform : null, transform.position, _radius, _minimumHeight, _maximumHeight, Random.Range(_minimumSpeed, _maximumSpeed), position);
            _seagulls[index] = seagull;
        }

        _nextCallTime = Time.time + Random.Range(2f, 6f);
    }

    private void Update()
    {
        if (_seagulls == null || _audioService == null || Time.time < _nextCallTime)
            return;

        SeagullFlight seagull = _seagulls[Random.Range(0, _seagulls.Length)];
        _audioService.PlaySeagullCall(seagull.transform.position);
        _nextCallTime = Time.time + Random.Range(_minimumCallInterval, _maximumCallInterval);
    }

    private Vector3 GetStartPosition(int index, Camera viewCamera)
    {
        if (index < 3 && viewCamera != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(viewCamera.transform.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            float distance = 35f + index * 6f;
            float horizontalOffset = (index - 1) * 13f;
            Vector3 position = viewCamera.transform.position + forward * distance + right * horizontalOffset;
            position.y = viewCamera.transform.position.y + _minimumHeight + index;
            return position;
        }

        Vector2 offset = Random.insideUnitCircle * _radius;
        Vector3 center = viewCamera != null ? viewCamera.transform.position : transform.position;
        return center + new Vector3(offset.x, Random.Range(_minimumHeight, _maximumHeight), offset.y);
    }

    private void OnValidate()
    {
        _maximumHeight = Mathf.Max(_minimumHeight, _maximumHeight);
        _maximumSpeed = Mathf.Max(_minimumSpeed, _maximumSpeed);
        _maximumCallInterval = Mathf.Max(_minimumCallInterval, _maximumCallInterval);
    }
}
