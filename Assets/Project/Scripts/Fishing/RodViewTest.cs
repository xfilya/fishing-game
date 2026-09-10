using UnityEngine;
using UnityEngine.InputSystem;

public sealed class RodViewTest : MonoBehaviour
{
    [SerializeField] private RodView _rodView;

    private bool _cast;

    private void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (_cast)
            _rodView.PlayReturn();
        else
            _rodView.PlayCast();

        _cast = !_cast;
    }
}