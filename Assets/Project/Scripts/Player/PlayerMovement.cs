using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float SpeedMultiplier { get; set; } = 1f;
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
}