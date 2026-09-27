using UnityEngine;
using VContainer;

[RequireComponent(typeof(PlayerMovement))]
public sealed class PlayerAudio : MonoBehaviour
{
    [SerializeField, Min(0.2f)] private float _stepDistance = 1.5f;
    [SerializeField, Min(0.1f)] private float _groundCheckDistance = 2.5f;
    [SerializeField] private LayerMask _groundMask = ~0;

    private AudioService _audio;
    private PlayerMovement _movement;
    private PlayerJump _jump;
    private Vector3 _lastPosition;
    private float _distanceSinceStep;
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

        if (grounded && _wasAirborne)
        {
            _audio.PlayLanding(IsStone());
            _wasAirborne = false;
            _distanceSinceStep = 0f;
        }
        else if (!grounded)
        {
            _wasAirborne = true;
            _distanceSinceStep = 0f;
        }

        if (!grounded || movement.magnitude > 4f)
            return;

        _distanceSinceStep += movement.magnitude;

        if (_distanceSinceStep < _stepDistance)
            return;

        _distanceSinceStep %= _stepDistance;
        _audio.PlayStep(IsStone());
    }

    private void OnJump()
    {
        _wasAirborne = true;
        _distanceSinceStep = 0f;
        _audio?.PlayJump();
    }

    private bool IsStone()
    {
        Vector3 origin = transform.position + Vector3.up * 0.4f;

        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _groundCheckDistance, _groundMask, QueryTriggerInteraction.Ignore))
            return false;

        if (hit.collider.TryGetComponent(out AudioSurface surface))
            return surface.IsStone;

        string objectName = hit.collider.name.ToLowerInvariant();
        return objectName.Contains("stone") || objectName.Contains("rock") || objectName.Contains("boulder") || objectName.Contains("cliff") || objectName.Contains("port") || objectName.Contains("pier");
    }
}
