using UnityEngine;

public sealed class SeagullFlight : MonoBehaviour
{
    [SerializeField] private Transform _leftWing;
    [SerializeField] private Transform _rightWing;
    [SerializeField, Min(0.1f)] private float _turnResponsiveness = 1.8f;
    [SerializeField, Min(0.1f)] private float _flapSpeed = 5.5f;
    [SerializeField, Range(0f, 60f)] private float _flapAngle = 32f;
    [SerializeField, Range(0f, 45f)] private float _maximumBankAngle = 18f;

    private Transform _areaCenter;
    private Vector3 _fallbackCenter;
    private Vector3 _target;
    private Vector3 _velocity;
    private float _radius;
    private float _minimumHeight;
    private float _maximumHeight;
    private float _speed;
    private float _flapOffset;

    public void Initialize(Transform areaCenter, Vector3 fallbackCenter, float radius, float minimumHeight, float maximumHeight, float speed, Vector3 startPosition)
    {
        _areaCenter = areaCenter;
        _fallbackCenter = fallbackCenter;
        _radius = radius;
        _minimumHeight = minimumHeight;
        _maximumHeight = maximumHeight;
        _speed = speed;
        _flapOffset = Random.Range(0f, Mathf.PI * 2f);
        _turnResponsiveness *= Random.Range(0.7f, 1.3f);
        _flapSpeed *= Random.Range(0.75f, 1.25f);
        _flapAngle *= Random.Range(0.65f, 1.15f);
        transform.position = startPosition;
        SelectTarget();
        _velocity = (_target - startPosition).normalized * _speed;
        transform.rotation = Quaternion.LookRotation(_velocity);
    }

    private void Update()
    {
        if (_speed <= 0f)
            return;

        if ((transform.position - _target).sqrMagnitude < 16f)
            SelectTarget();

        Vector3 desiredVelocity = (_target - transform.position).normalized * _speed;
        float steering = 1f - Mathf.Exp(-_turnResponsiveness * Time.deltaTime);
        _velocity = Vector3.Lerp(_velocity, desiredVelocity, steering);
        transform.position += _velocity * Time.deltaTime;

        if (_velocity.sqrMagnitude > 0.001f)
        {
            float turn = Vector3.SignedAngle(transform.forward, _velocity, Vector3.up);
            float bank = Mathf.Clamp(-turn * 1.5f, -_maximumBankAngle, _maximumBankAngle);
            Quaternion rotation = Quaternion.LookRotation(_velocity) * Quaternion.Euler(0f, 0f, bank);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, steering);
        }

        float flap = Mathf.Sin(Time.time * _flapSpeed + _flapOffset) * _flapAngle;
        _leftWing.localRotation = Quaternion.Euler(0f, 0f, -flap);
        _rightWing.localRotation = Quaternion.Euler(0f, 0f, flap);
    }

    private void SelectTarget()
    {
        Vector2 offset = Random.insideUnitCircle * _radius;
        Vector3 center = _areaCenter != null ? _areaCenter.position : _fallbackCenter;
        _target = center + new Vector3(offset.x, Random.Range(_minimumHeight, _maximumHeight), offset.y);
    }
}
