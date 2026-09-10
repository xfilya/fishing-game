using DG.Tweening;
using UnityEngine;

public sealed class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    private Tween _fovTween;
    private float _defaultFov;

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
        _defaultFov = _camera.fieldOfView;
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

    private void OnDestroy()
    {
        _fovTween?.Kill();
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

    public void SetSprintFov(bool isSprinting)
    {
        float targetFov = isSprinting ? _defaultFov + _config.SprintFovOffset : _defaultFov;

        _fovTween?.Kill();

        _fovTween = _camera.DOFieldOfView(targetFov, _config.SprintTransitionSpeed).SetEase(Ease.OutSine);
    }
}