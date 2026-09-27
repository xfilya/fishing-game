using UnityEngine;
using VContainer;

[RequireComponent(typeof(PlayerMovement))]
public sealed class PlayerAudio : MonoBehaviour
{
    [SerializeField, Min(0.2f)] private float _stepDistance = 1.5f;
    [SerializeField, Min(0.2f)] private float _sandStepDistance = 1.85f;
    [SerializeField, Min(0.05f)] private float _minimumStepInterval = 0.27f;
    [SerializeField, Min(0.05f)] private float _minimumSandStepInterval = 0.37f;
    [SerializeField, Min(0.05f)] private float _minimumAirTime = 0.35f;
    [SerializeField, Min(0.05f)] private float _minimumLandingInterval = 0.6f;
    [SerializeField, Min(0.1f)] private float _groundCheckDistance = 2.5f;
    [SerializeField] private LayerMask _groundMask = ~0;

    private AudioService _audio;
    private PlayerMovement _movement;
    private PlayerJump _jump;
    private Vector3 _lastPosition;
    private float _distanceSinceStep;
    private float _ungroundedTime;
    private float _lastStepTime = -10f;
    private float _lastLandingTime = -10f;
    private bool _wasAirborne;

    [Inject]
    public void Construct(AudioService audio) => _audio = audio;

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _jump = GetComponent<PlayerJump>();
    }

    private void OnEnable()
    {
        _lastPosition = transform.position;
        _jump.OnJump += OnJump;
    }

    private void OnDisable() => _jump.OnJump -= OnJump;

    private void LateUpdate()
    {
        if (_audio == null)
            return;

        bool grounded = _movement.CharacterController.isGrounded && _movement.VerticalVelocity <= 0f;
        Vector3 position = transform.position;
        Vector2 movement = new(position.x - _lastPosition.x, position.z - _lastPosition.z);
        _lastPosition = position;

        if (!grounded)
        {
            _ungroundedTime += Time.deltaTime;

            if (_movement.VerticalVelocity > 0.1f || _ungroundedTime >= _minimumAirTime)
            {
                _wasAirborne = true;
                _distanceSinceStep = 0f;
            }

            return;
        }

        _ungroundedTime = 0f;

        if (_wasAirborne)
        {
            if (Time.time - _lastLandingTime >= _minimumLandingInterval)
            {
                _audio.PlayLanding(GetSurface());
                _lastLandingTime = Time.time;
            }

            _wasAirborne = false;
            _distanceSinceStep = 0f;
        }

        if (movement.magnitude > 4f || movement.magnitude < Time.deltaTime * 0.25f)
            return;

        _distanceSinceStep += movement.magnitude;
        AudioSurfaceType surface = GetSurface();
        float stepDistance = surface == AudioSurfaceType.Sand ? _sandStepDistance : _stepDistance;
        float stepInterval = surface == AudioSurfaceType.Sand ? _minimumSandStepInterval : _minimumStepInterval;

        if (_distanceSinceStep < stepDistance || Time.time - _lastStepTime < stepInterval)
            return;

        _distanceSinceStep = 0f;
        _lastStepTime = Time.time;
        _audio.PlayStep(surface);
    }

    private void OnJump()
    {
        _wasAirborne = true;
        _ungroundedTime = _minimumAirTime;
        _distanceSinceStep = 0f;
        _audio?.PlayJump();
    }

    private AudioSurfaceType GetSurface()
    {
        Vector3 origin = transform.position + Vector3.up * 0.4f;

        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _groundCheckDistance, _groundMask, QueryTriggerInteraction.Ignore))
            return AudioSurfaceType.Sand;

        AudioSurface surface = hit.collider.GetComponentInParent<AudioSurface>();

        if (surface != null)
            return surface.SurfaceType;

        string objectName = hit.collider.name.ToLowerInvariant();

        if (objectName.Contains("port") || objectName.Contains("pier") || objectName.Contains("dock") || objectName.Contains("wood") || objectName.Contains("plank"))
            return AudioSurfaceType.Wood;

        return objectName.Contains("stone") || objectName.Contains("rock") || objectName.Contains("boulder") || objectName.Contains("cliff") ? AudioSurfaceType.Stone : AudioSurfaceType.Sand;
    }
}
