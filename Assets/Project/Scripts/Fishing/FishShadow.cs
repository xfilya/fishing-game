using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public sealed class FishShadow : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float _minimumSwimSpeed = 0.7f;
    [SerializeField, Min(0.01f)] private float _maximumSwimSpeed = 1.35f;
    [SerializeField, Min(0.01f)] private float _attractionSpeed = 3.5f;
    [SerializeField, Min(0.01f)] private float _attractionSmoothTime = 0.7f;
    [SerializeField, Min(0.01f)] private float _targetDistance = 0.08f;
    [SerializeField, Min(0.01f)] private float _minimumDirectionDuration = 1.5f;
    [SerializeField, Min(0.01f)] private float _maximumDirectionDuration = 4f;
    [SerializeField] private float _surfaceOffset = 0.12f;
    [SerializeField, Range(0f, 0.25f)] private float _pulseAmount = 0.08f;
    [SerializeField, Min(0.1f)] private float _pulseSpeed = 2f;

    private FishShadowSpawner _owner;
    private FishingWater _water;
    private Bobber _target;
    private Action<FishShadow> _reachedTarget;
    private Vector3 _direction;
    private Vector3 _baseScale;
    private Vector3 _homePosition;
    private Vector3 _attractionVelocity;
    private float _speed;
    private float _nextDirectionTime;
    private bool _attracted;
    private bool _arrived;

    public bool IsAvailable => gameObject.activeInHierarchy && !_attracted;

    public void Initialize(FishShadowSpawner owner, FishingWater water, Vector3 position)
    {
        _owner = owner;
        _water = water;
        _baseScale = transform.localScale;
        ResetRoaming(position);
    }

    public void Attract(Bobber target, Action<FishShadow> reachedTarget)
    {
        _target = target;
        _reachedTarget = reachedTarget;
        _attractionVelocity = Vector3.zero;
        _attracted = true;
        _arrived = false;
    }

    public void Release()
    {
        _target = null;
        _reachedTarget = null;
        _attractionVelocity = Vector3.zero;
        _attracted = false;
        _arrived = false;
        SelectDirection();
    }

    public void Consume()
    {
        _target = null;
        _reachedTarget = null;
        _attractionVelocity = Vector3.zero;
        _attracted = false;
        _arrived = false;
        gameObject.SetActive(false);
        _owner.NotifyConsumed(this);
    }

    public void ResetRoaming(Vector3 position)
    {
        gameObject.SetActive(true);
        transform.position = WithSurfaceHeight(position);
        _homePosition = transform.position;
        _target = null;
        _reachedTarget = null;
        _attractionVelocity = Vector3.zero;
        _attracted = false;
        _arrived = false;
        SelectDirection();
    }

    private void Update()
    {
        transform.localScale = _baseScale * (1f + Mathf.Sin(Time.time * _pulseSpeed) * _pulseAmount);

        if (_attracted)
            UpdateAttraction();
        else
            UpdateRoaming();
    }

    private void UpdateRoaming()
    {
        if (Time.time >= _nextDirectionTime)
            SelectDirection();

        Vector3 nextPosition = transform.position + _direction * (_speed * Time.deltaTime);

        if (!_owner.IsRoamingPositionValid(nextPosition, _homePosition))
        {
            _direction = _owner.GetDirectionToHome(transform.position, _homePosition);
            nextPosition = transform.position + _direction * (_speed * Time.deltaTime);
        }

        transform.position = WithSurfaceHeight(nextPosition);
    }

    private void UpdateAttraction()
    {
        if (_target == null)
        {
            Release();
            return;
        }

        Vector3 targetPosition = WithSurfaceHeight(_target.transform.position);

        if (_arrived)
        {
            transform.position = targetPosition;
            return;
        }

        Vector3 nextPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref _attractionVelocity, _attractionSmoothTime, _attractionSpeed, Time.deltaTime);
        transform.position = WithSurfaceHeight(nextPosition);

        if ((transform.position - targetPosition).sqrMagnitude > _targetDistance * _targetDistance)
            return;

        _arrived = true;
        transform.position = targetPosition;
        Action<FishShadow> reachedTarget = _reachedTarget;
        _reachedTarget = null;
        reachedTarget?.Invoke(this);
    }

    private void SelectDirection()
    {
        Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
        _direction = new Vector3(direction.x, 0f, direction.y);
        _speed = UnityEngine.Random.Range(_minimumSwimSpeed, _maximumSwimSpeed);
        _nextDirectionTime = Time.time + UnityEngine.Random.Range(_minimumDirectionDuration, _maximumDirectionDuration);
    }

    private Vector3 WithSurfaceHeight(Vector3 position)
    {
        position.y = _water.GetHeight(position) + _surfaceOffset;
        return position;
    }

    private void OnValidate()
    {
        _maximumSwimSpeed = Mathf.Max(_minimumSwimSpeed, _maximumSwimSpeed);
        _maximumDirectionDuration = Mathf.Max(_minimumDirectionDuration, _maximumDirectionDuration);
    }
}
