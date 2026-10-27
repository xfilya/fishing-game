using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputService : IInputService, IDisposable
{
    private readonly InputSystem_Actions _actions;
    private readonly MobileInputService _mobile;

    private bool _gameplayEnabled = true;

    public bool IsTouch => _mobile.IsActive;
    public bool IsGameplayEnabled => _gameplayEnabled;

    public bool IsMouse =>
        !IsTouch && _actions.Player.Look.activeControl?.device is Mouse;
        
    public Vector2 Movement =>
        _gameplayEnabled
        ? (IsTouch ? _mobile.Movement : _actions.Player.Move.ReadValue<Vector2>())
        : Vector2.zero;

    public Vector2 Look =>
        _gameplayEnabled
            ? (IsTouch ? _mobile.Look : _actions.Player.Look.ReadValue<Vector2>())
            : Vector2.zero;

    public bool JumpPressedThisFrame =>
        _gameplayEnabled &&
        (IsTouch ? _mobile.JumpPressed : _actions.Player.Jump.WasPressedThisFrame());

    public bool SprintHeld =>
        _gameplayEnabled &&
        (IsTouch ? _mobile.SprintHeld : _actions.Player.Sprint.IsPressed());

    public bool PrimaryActionPressedThisFrame =>
        _gameplayEnabled &&
        (IsTouch ? _mobile.PrimaryPressed : _actions.Player.Attack.WasPressedThisFrame());

    public bool InteractPressedThisFrame =>
        IsTouch ? _mobile.InteractPressed : _actions.Player.Interact.WasPressedThisFrame();

    public bool EscapePressedThisFrame =>
        _actions.Player.Escape.WasPressedThisFrame() || IsTouch && _mobile.PausePressed;



    public void SetGameplayEnabled(bool isEnabled)
    {
        _gameplayEnabled = isEnabled;
        if (!isEnabled)
            _mobile.ResetGameplay();
    }

    public InputService(MobileInputService mobile)
    {
        _mobile = mobile;
        _actions = new InputSystem_Actions();

        _actions.Player.Enable();
    }

    public void Dispose()
    {
        _actions.Player.Disable();
        _actions.Dispose();
    }
}
