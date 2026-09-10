using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputService : IInputService, IDisposable
{
    private readonly InputSystem_Actions _actions;

    public Vector2 Movement =>
        _actions.Player.Move.ReadValue<Vector2>();

    public Vector2 Look => 
        _actions.Player.Look.ReadValue<Vector2>();

    public bool IsMouse => 
        _actions.Player.Look.activeControl?.device is Mouse;

    public bool JumpPressedThisFrame => 
        _actions.Player.Jump.WasPressedThisFrame();

    public bool SprintHeld => 
        _actions.Player.Sprint.IsPressed();

    public bool EscapePressedThisFrame =>
        _actions.Player.Escape.WasPressedThisFrame();

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