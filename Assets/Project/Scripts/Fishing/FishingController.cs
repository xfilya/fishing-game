using UnityEngine;
using VContainer;

public enum FishingState
{
    Ready,
    Casting,
    Waiting,
    BiteWindow,
    Reeling,
    Result
}

public sealed class FishingController : MonoBehaviour
{
    [SerializeField] private RodView _rodView;

    private IInputService _input;
    private FishingState _state = FishingState.Ready;

    [Inject]
    public void Construct(IInputService inputService)
    {
        _input = inputService;
    }

    private void OnEnable()
    {
        _rodView.CastReleased += OnCastReleased;
        _rodView.Returned += OnRodReturned;
    }

    private void OnDisable()
    {
        _rodView.CastReleased -= OnCastReleased;
        _rodView.Returned -= OnRodReturned;
    }

    private void Update()
    {
        if (_input == null)
            return;

        if (_input.PrimaryActionPressedThisFrame)
        {
            HandlePrimaryAction();
        }
    }

    private void HandlePrimaryAction()
    {
        switch (_state)
        {
            case FishingState.Ready:
                BeginCast();
                break;

            case FishingState.Waiting:
                // нажатие слишком рано.
                BeginReturn(caughtFish: false);
                break;

            case FishingState.BiteWindow:
                // нажатие вовремя.
                BeginReturn(caughtFish: true);
                break;

            case FishingState.Casting:
            case FishingState.Reeling:
            case FishingState.Result:
                break;
        }
    }

    private void BeginCast()
    {
        _state = FishingState.Casting;
        _rodView.PlayCast();
    }

    private void OnCastReleased()
    {
        // поплавок заглушка
    }

    public void NotifyBobberEnteredWater()
    {
        _state = FishingState.Waiting;
    }

    public void NotifyBiteStarted()
    {
        if (_state != FishingState.Waiting)
            return;

        _state = FishingState.BiteWindow;

        // поплавок дергается вниз заглушка
    }

    private void BeginReturn(bool caughtFish)
    {
        _state = FishingState.Reeling;
        _rodView.PlayReturn();

        // здесь начинаем сматывать леску заглушка
    }

    private void OnRodReturned()
    {
        _state = FishingState.Ready;
    }
}