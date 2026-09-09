using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMovement : MonoBehaviour
{
    public float SpeedMultiplier { get; private set; } = 1f;
    public CharacterController CharacterController { get; private set; }
    public float VerticalVelocity { get; private set; }
    public float MoveSpeed => _config.BaseSpeed * SpeedMultiplier;


    private PlayerConfig _config;
    private IInputService _inputService;

    private void Awake()
    {
        CharacterController = GetComponent<CharacterController>();
    }

    public void Initialize(
        IInputService inputService,
        PlayerConfig config
    )
    {
        _inputService = inputService;
        _config = config;
    }

    private void Update()
    {
        if (_inputService == null || _config == null)
            return;

        Move();
    }

    private void Move()
    {
        Vector2 inputDirection = _inputService.Movement;
        Vector3 direction = transform.right * inputDirection.x + transform.forward * inputDirection.y;

        direction = Vector3.ClampMagnitude(direction, 1f);

        if (CharacterController.isGrounded && VerticalVelocity < 0f)
        {
            VerticalVelocity = -2f;
        }

        VerticalVelocity += _config.Gravity * Time.deltaTime;

        Vector3 velocity = direction * MoveSpeed;

        velocity.y = VerticalVelocity;

        CharacterController.Move(velocity * Time.deltaTime);
    }

    public bool TryJump(float jumpHeight)
    {
        if (!CharacterController.isGrounded)
            return false;

        VerticalVelocity = Mathf.Sqrt(jumpHeight * -2f * _config.Gravity);

        return true;
    }

    public void SetSpeedMultiplier(float value) => SpeedMultiplier = Mathf.Max(0f, value);

    public void Teleport(Transform point)
    {
        CharacterController.enabled = false;

        transform.SetPositionAndRotation(point.position, point.rotation);
        VerticalVelocity = 0f;

        CharacterController.enabled = true;
    }
    
}