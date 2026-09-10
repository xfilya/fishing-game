using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputService : IInputService, IDisposable
{
    private readonly InputSystem_Actions _actions;

    private bool _gameplayEnabled = true;

    public bool IsMouse =>
        _actions.Player.Look.activeControl?.device is Mouse;
        
    public Vector2 Movement =>
        _gameplayEnabled
        ? _actions.Player.Move.ReadValue<Vector2>()
        : Vector2.zero;

    public Vector2 Look =>
        _gameplayEnabled
            ? _actions.Player.Look.ReadValue<Vector2>()
            : Vector2.zero;

    public bool JumpPressedThisFrame =>
        _gameplayEnabled &&
        _actions.Player.Jump.WasPressedThisFrame();

    public bool SprintHeld =>
        _gameplayEnabled &&
        _actions.Player.Sprint.IsPressed();

    public bool PrimaryActionPressedThisFrame =>
        _gameplayEnabled &&
        _actions.Player.Attack.WasPressedThisFrame();

    public bool EscapePressedThisFrame =>
        _actions.Player.Escape.WasPressedThisFrame();



    public void SetGameplayEnabled(bool isEnabled)
    {
        _gameplayEnabled = isEnabled;
    }

    public InputService()
    {
        _actions = new InputSystem_Actions();

        _actions.Player.Enable();
    }

    public void Dispose()
    {
        _actions.Player.Disable();
        _actions.Dispose();
    }
}