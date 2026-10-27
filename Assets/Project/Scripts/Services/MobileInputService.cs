using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class MobileInputService : IDisposable
{
    private readonly bool _mobileDevice;
    private bool _simulateTouch;
    private Vector2 _pendingLook;
    private Vector2 _look;
    private bool _pendingJump;
    private bool _pendingPrimary;
    private bool _pendingInteract;
    private bool _pendingPause;
    private bool _jumpPressed;
    private bool _primaryPressed;
    private bool _interactPressed;
    private bool _pausePressed;
    private int _readFrame = -1;
    private float _lastTouchTime = -1f;

    public bool IsActive { get; private set; }
    public Vector2 Movement { get; private set; }
    public bool SprintHeld { get; private set; }
    public Vector2 Look { get { ReadFrame(); return _look; } }
    public bool JumpPressed { get { ReadFrame(); return _jumpPressed; } }
    public bool PrimaryPressed { get { ReadFrame(); return _primaryPressed; } }
    public bool InteractPressed { get { ReadFrame(); return _interactPressed; } }
    public bool PausePressed { get { ReadFrame(); return _pausePressed; } }

#if UNITY_WEBGL && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern int IsTouchBrowser();
#endif

    public MobileInputService()
    {
        _mobileDevice = Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld;
#if UNITY_WEBGL && !UNITY_EDITOR
        _mobileDevice |= IsTouchBrowser() != 0;
#endif
        IsActive = _mobileDevice || Touchscreen.current != null && Mouse.current == null;
        InputSystem.onAfterUpdate += UpdateDevice;
    }

    public void SetMovement(Vector2 value) => Movement = Vector2.ClampMagnitude(value, 1f);
    public void AddLook(Vector2 value) => _pendingLook += value;
    public void PressJump() => _pendingJump = true;
    public void PressPrimary() => _pendingPrimary = true;
    public void PressInteract() => _pendingInteract = true;
    public void PressPause() => _pendingPause = true;
    public void ToggleSprint() => SprintHeld = !SprintHeld;

    public void SetEditorSimulation(bool isEnabled)
    {
#if UNITY_EDITOR
        _simulateTouch = isEnabled;
        SetActive(_simulateTouch || _mobileDevice);
#endif
    }

    public void ResetGameplay()
    {
        Movement = Vector2.zero;
        SprintHeld = false;
        _pendingLook = Vector2.zero;
        _look = Vector2.zero;
        _pendingJump = _pendingPrimary = _pendingInteract = _pendingPause = false;
        _jumpPressed = _primaryPressed = _interactPressed = _pausePressed = false;
    }

    public void Dispose() => InputSystem.onAfterUpdate -= UpdateDevice;

    private void ReadFrame()
    {
        if (_readFrame == Time.frameCount)
            return;

        _readFrame = Time.frameCount;
        _look = _pendingLook;
        _jumpPressed = _pendingJump;
        _primaryPressed = _pendingPrimary;
        _interactPressed = _pendingInteract;
        _pausePressed = _pendingPause;
        _pendingLook = Vector2.zero;
        _pendingJump = _pendingPrimary = _pendingInteract = _pendingPause = false;
    }

    private void UpdateDevice()
    {
        if (_mobileDevice || _simulateTouch)
        {
            SetActive(true);
            return;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            _lastTouchTime = Time.realtimeSinceStartup;
            SetActive(true);
            return;
        }

        bool keyboardUsed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool gamepadUsed = Gamepad.current != null && (Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.04f || Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.04f || Gamepad.current.buttonSouth.wasPressedThisFrame);
        bool mouseUsed = Mouse.current != null && Time.realtimeSinceStartup - _lastTouchTime > 0.8f && (Mouse.current.delta.ReadValue().sqrMagnitude > 1f || Mouse.current.leftButton.wasPressedThisFrame);
        if (keyboardUsed || gamepadUsed || mouseUsed)
            SetActive(false);
    }

    private void SetActive(bool isActive)
    {
        if (IsActive == isActive)
            return;

        ResetGameplay();
        IsActive = isActive;
    }
}
