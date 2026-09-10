using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(FishingLineView))]
public sealed class Bobber : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Transform _buoyancyPoint;
    [SerializeField] private Transform _lineAnchor;
    [SerializeField] private FishingLineView _lineView;
    [SerializeField, Min(0f)] private float _waterSlack = 0.35f;
    [SerializeField, Min(0f)] private float _buoyancy = 300f;
    [SerializeField, Min(0f)] private float _waterDrag = 12f;
    [SerializeField, Min(0f)] private float _floatingDepth = 0.01f;
    [SerializeField, Min(0f)] private float _tensionStiffness = 60f;
    [SerializeField, Min(0f)] private float _tensionDamping = 8f;
    [SerializeField, Min(0f)] private float _maxTensionAcceleration = 100f;
    [SerializeField, Min(0.01f)] private float _maxSubmersionDepth = 0.05f;
    [SerializeField, Min(0.01f)] private float _retrieveDistance = 0.25f;

    private FishingWater _water;
    private FishingWater _targetWater;
    private Transform _lineOrigin;
    private float _lineLength;
    private float _reelSpeed;
    private bool _isDeploying;
    private bool _isReeling;
    private bool _retrieved;
    private bool _hasEnteredWater;

    public event Action<Bobber> WaterEntered;
    public event Action<Bobber> Retrieved;

    public Transform LineAnchor => _lineAnchor;

    private void Awake()
    {
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();

        if (_buoyancyPoint == null)
            _buoyancyPoint = transform;

        if (_lineAnchor == null)
            _lineAnchor = transform;

        if (_lineView == null)
            _lineView = GetComponent<FishingLineView>();

        _rigidbody.centerOfMass = new Vector3(0f, -0.04f, 0f);
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
        _lineView.Detach();
    }

    public void Launch(Transform lineOrigin, Vector3 target, float flightDuration, FishingWater targetWater)
    {
        _lineOrigin = lineOrigin;
        _water = null;
        _targetWater = targetWater;
        _isDeploying = true;
        _isReeling = false;
        _retrieved = false;
        _hasEnteredWater = false;
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        transform.position = lineOrigin.position;
        transform.rotation = Quaternion.identity;
        _lineLength = 0.05f;
        _lineView.Attach(_lineOrigin, _lineAnchor, _lineLength);
        _rigidbody.linearVelocity = CalculateLaunchVelocity(transform.position, target, flightDuration);
        _rigidbody.angularVelocity = Vector3.zero;
    }

    public void BeginReel(float reelSpeed)
    {
        _reelSpeed = reelSpeed;
        _isDeploying = false;
        _isReeling = true;
        _water = null;
        _rigidbody.useGravity = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    public void Dip(float impulse)
    {
        if (_retrieved)
            return;

        _rigidbody.AddForce(Vector3.down * impulse, ForceMode.VelocityChange);
    }

    private void FixedUpdate()
    {
        if (_lineOrigin == null || _retrieved)
            return;

        if (_water == null && !_isReeling && _targetWater != null && _buoyancyPoint.position.y <= _targetWater.GetHeight(_buoyancyPoint.position))
            EnterWater(_targetWater);

        if (_isReeling)
        {
            Reel();
            return;
        }

        float distance = Vector3.Distance(_lineOrigin.position, _lineAnchor.position);

        if (_isDeploying)
            _lineLength = Mathf.Max(_lineLength, distance);

        if (_water != null)
            ApplyBuoyancy();

        if (!_isDeploying)
            ApplyTension(distance);

        _lineView.SetLineLength(_lineLength);
    }

    private void Reel()
    {
        Vector3 target = _lineOrigin.position;
        Vector3 nextPosition = Vector3.MoveTowards(_rigidbody.position, target, _reelSpeed * Time.fixedDeltaTime);
        float distance = Vector3.Distance(nextPosition, target);
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.MovePosition(nextPosition);
        _lineLength = distance;
        _lineView.SetLineLength(_lineLength);

        if (distance <= _retrieveDistance)
            CompleteRetrieve();
    }

    private void ApplyBuoyancy()
    {
        float surfaceHeight = _water.GetHeight(_buoyancyPoint.position);
        float depth = surfaceHeight - _buoyancyPoint.position.y;

        if (depth <= 0f)
            return;

        if (depth > _maxSubmersionDepth)
        {
            _rigidbody.position += Vector3.up * (depth - _maxSubmersionDepth);
            Vector3 velocity = _rigidbody.linearVelocity;
            _rigidbody.linearVelocity = new Vector3(velocity.x, Mathf.Max(0f, velocity.y), velocity.z);
            depth = _maxSubmersionDepth;
        }

        Vector3 pointVelocity = _rigidbody.GetPointVelocity(_buoyancyPoint.position);
        float targetHeight = surfaceHeight - _floatingDepth;
        float force = -Physics.gravity.y + (targetHeight - _buoyancyPoint.position.y) * _buoyancy - pointVelocity.y * _waterDrag;
        _rigidbody.AddForceAtPosition(Vector3.up * Mathf.Max(0f, force), _buoyancyPoint.position, ForceMode.Acceleration);
        _rigidbody.AddForce(-new Vector3(pointVelocity.x, 0f, pointVelocity.z) * _waterDrag, ForceMode.Acceleration);
    }

    private void ApplyTension(float distance)
    {
        if (distance <= _lineLength || distance <= Mathf.Epsilon)
            return;

        Vector3 direction = (_lineOrigin.position - _lineAnchor.position) / distance;
        float velocityAlongLine = Vector3.Dot(_rigidbody.linearVelocity, direction);
        float acceleration = (distance - _lineLength) * _tensionStiffness - velocityAlongLine * _tensionDamping;
        _rigidbody.AddForce(direction * Mathf.Clamp(acceleration, 0f, _maxTensionAcceleration), ForceMode.Acceleration);
    }

    private Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 target, float duration)
    {
        Vector3 displacement = target - start;
        Vector3 horizontal = new(displacement.x, 0f, displacement.z);
        float verticalVelocity = (displacement.y - Physics.gravity.y * duration * duration * 0.5f) / duration;
        return horizontal / duration + Vector3.up * verticalVelocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_lineOrigin == null || _isReeling)
            return;

        FishingWater water = other.GetComponentInParent<FishingWater>();

        if (water == null)
            return;

        EnterWater(water);
    }

    private void EnterWater(FishingWater water)
    {
        _water = water;
        StabilizeAtWaterSurface();

        if (_hasEnteredWater)
            return;

        _hasEnteredWater = true;
        _isDeploying = false;
        _lineLength = Vector3.Distance(_lineOrigin.position, _lineAnchor.position) + _waterSlack;
        WaterEntered?.Invoke(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_water != null && other.GetComponentInParent<FishingWater>() == _water && _buoyancyPoint.position.y > _water.GetHeight(_buoyancyPoint.position) + _maxSubmersionDepth)
            _water = null;
    }

    private void StabilizeAtWaterSurface()
    {
        float depth = _water.GetHeight(_buoyancyPoint.position) - _buoyancyPoint.position.y;

        if (depth > _maxSubmersionDepth)
            _rigidbody.position += Vector3.up * (depth - _maxSubmersionDepth);

        Vector3 velocity = _rigidbody.linearVelocity;
        _rigidbody.linearVelocity = new Vector3(velocity.x, Mathf.Max(velocity.y * 0.1f, -1f), velocity.z);
    }

    private void CompleteRetrieve()
    {
        _retrieved = true;
        _isReeling = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.isKinematic = true;
        _lineView.Detach();
        gameObject.SetActive(false);
        Retrieved?.Invoke(this);
    }

    private void OnDisable()
    {
        if (_lineView != null)
            _lineView.Detach();
    }
}
