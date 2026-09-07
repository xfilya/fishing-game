using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform _cameraRoot;

    private IInputService _input;
    private PlayerConfig _config;

    private float pitch;

    public void Initialize(
        IInputService inputService,
        PlayerConfig playerConfig)
    {
        _input = inputService;
        _config = playerConfig;
    }

    private void Start()
    {
        if (!Application.isMobilePlatform)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        if (_input == null)
            return;

        Look();
    }

    private void Look()
    {

        Vector2 look = _input.Look;

        float yaw;
        float pitchDelta;

        if (_input.IsMouse)
        {
            yaw = look.x * _config.MouseSensitivity;

            pitchDelta = look.y * _config.MouseSensitivity;
        }
        else
        {
            yaw = look.x * _config.StickSensitivity * Time.deltaTime;

            pitchDelta = look.y * _config.StickSensitivity * Time.deltaTime;
        }

        transform.Rotate(Vector3.up * yaw);

        pitch -= pitchDelta;

        pitch = Mathf.Clamp(pitch, -_config.CameraClampAngle, _config.CameraClampAngle);

        _cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}