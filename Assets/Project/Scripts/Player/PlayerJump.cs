using System;
using UnityEngine;

public sealed class PlayerJump : MonoBehaviour
{
    public event Action OnJump;
    public event Action OnLand;

    private IInputService _input;
    private PlayerConfig _config;
    private PlayerMovement _movement;

    public void Initialize(
        IInputService inputService,
        PlayerConfig playerConfig,
        PlayerMovement playerMovement)
    {
        _input = inputService;
        _config = playerConfig;
        _movement = playerMovement;
    }

    private void Update()
    {
        if (_input == null)
            return;

       Jump();
    }

    private void Jump()
    {
        if (_input.JumpPressedThisFrame && _movement.TryJump(_config.JumpHeight))
        {
            OnJump?.Invoke();
        }
    }
}