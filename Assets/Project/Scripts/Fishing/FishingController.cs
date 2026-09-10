using System;
using System.Collections;
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
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _lineOrigin;
    [SerializeField] private Bobber _bobberPrefab;
    [SerializeField] private LayerMask _castMask = ~0;
    [SerializeField, Min(1f)] private float _maxCastDistance = 30f;
    [SerializeField, Min(0.1f)] private float _flightDuration = 0.7f;
    [SerializeField, Min(0.1f)] private float _reelSpeed = 15f;
    [SerializeField, Min(0.1f)] private float _castTimeout = 3f;
    [SerializeField, Min(0.1f)] private float _biteWindowDuration = 1f;
    [SerializeField, Min(0f)] private float _biteImpulse = 2.5f;

    private IInputService _input;
    private FishingState _state = FishingState.Ready;
    private Bobber _bobber;
    private Vector3 _castTarget;
    private FishingWater _castWater;
    private Coroutine _castTimeoutRoutine;
    private Coroutine _biteWindowRoutine;
    private bool _rodReturned;
    private bool _bobberReturned;
    private bool _caughtFish;

    public event Action CatchCompleted;

    public FishingState State => _state;

    [Inject]
    public void Construct(IInputService inputService)
    {
        _input = inputService;
    }

    private void Awake()
    {
        if (_rodView == null)
            _rodView = GetComponentInChildren<RodView>(true);

        if (_camera == null)
            _camera = GetComponentInChildren<Camera>(true);

        if (_lineOrigin == null)
            _lineOrigin = FindChild("LineOrigin");

        if (_rodView != null && _camera != null && _lineOrigin != null && _bobberPrefab != null)
            return;

        enabled = false;
    }

    private void OnEnable()
    {
        ResetRuntimeState();

        if (_rodView == null)
            return;

        _rodView.CastReleased += OnCastReleased;
        _rodView.Returned += OnRodReturned;
    }

    private void OnDisable()
    {
        if (_rodView != null)
        {
            _rodView.CastReleased -= OnCastReleased;
            _rodView.Returned -= OnRodReturned;
        }

        StopFishingRoutines();

        if (_bobber != null)
        {
            _bobber.WaterEntered -= OnBobberEnteredWater;
            _bobber.Retrieved -= OnBobberRetrieved;
            Destroy(_bobber.gameObject);
            _bobber = null;
        }

        ResetRuntimeState();
    }

    private void Start()
    {
        if (_input != null)
            return;

        enabled = false;
    }

    private void Update()
    {
        if (_input == null)
            return;

        if (_input.PrimaryActionPressedThisFrame)
            HandlePrimaryAction();
    }

    private void HandlePrimaryAction()
    {
        switch (_state)
        {
            case FishingState.Ready:
                BeginCast();
                break;

            case FishingState.Waiting:
                BeginReturn(false);
                break;

            case FishingState.BiteWindow:
                BeginReturn(true);
                break;

            case FishingState.Result:
                CompleteResult();
                break;

            case FishingState.Casting:
            case FishingState.Reeling:
                break;
        }
    }

    private void BeginCast()
    {
        if (!TryGetCastTarget(out _castTarget, out _castWater))
        {
            Debug.LogWarning($"[FishingController] Заброс отменён: центр камеры не попал в FishingWater на Cast Mask в пределах {_maxCastDistance} метров");
            return;
        }

        _rodReturned = false;
        _bobberReturned = false;
        _caughtFish = false;
        _state = FishingState.Casting;
        _rodView.PlayCast();
    }

    private void OnCastReleased()
    {
        if (_state != FishingState.Casting || _bobber != null)
            return;

        _bobber = Instantiate(_bobberPrefab, _lineOrigin.position, Quaternion.identity);
        _bobber.WaterEntered += OnBobberEnteredWater;
        _bobber.Retrieved += OnBobberRetrieved;
        _bobber.Launch(_lineOrigin, _castTarget, _flightDuration, _castWater);
        _castTimeoutRoutine = StartCoroutine(CastTimeoutRoutine());
    }

    private void OnBobberEnteredWater(Bobber bobber)
    {
        if (bobber != _bobber || _state != FishingState.Casting)
            return;

        StopRoutine(ref _castTimeoutRoutine);
        _state = FishingState.Waiting;
    }

    [ContextMenu("Test Bite")]
    public void NotifyBiteStarted()
    {
        if (_state != FishingState.Waiting)
            return;

        _state = FishingState.BiteWindow;
        _bobber.Dip(_biteImpulse);
        _biteWindowRoutine = StartCoroutine(BiteWindowRoutine());
    }

    private void BeginReturn(bool caughtFish)
    {
        if (_state == FishingState.Reeling || _state == FishingState.Ready || _state == FishingState.Result)
            return;

        StopFishingRoutines();
        _caughtFish = caughtFish;
        _state = FishingState.Reeling;
        _rodView.PlayReturn();

        if (_bobber != null)
            _bobber.BeginReel(_reelSpeed);
        else
            _bobberReturned = true;
    }

    private void OnRodReturned()
    {
        _rodReturned = true;
        TryCompleteReturn();
    }

    private void OnBobberRetrieved(Bobber bobber)
    {
        if (bobber != _bobber)
            return;

        _bobber.WaterEntered -= OnBobberEnteredWater;
        _bobber.Retrieved -= OnBobberRetrieved;
        Destroy(_bobber.gameObject);
        _bobber = null;
        _bobberReturned = true;
        TryCompleteReturn();
    }

    private void TryCompleteReturn()
    {
        if (_state != FishingState.Reeling || !_rodReturned || !_bobberReturned)
            return;

        if (_caughtFish)
        {
            _state = FishingState.Result;
            CatchCompleted?.Invoke();
            return;
        }

        _state = FishingState.Ready;
    }

    public void CompleteResult()
    {
        if (_state == FishingState.Result)
            _state = FishingState.Ready;
    }

    private bool TryGetCastTarget(out Vector3 target, out FishingWater water)
    {
        Ray ray = new(_camera.transform.position, _camera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, _maxCastDistance, _castMask, QueryTriggerInteraction.Collide))
        {
            water = hit.collider.GetComponentInParent<FishingWater>();

            if (water != null)
            {
                target = new Vector3(hit.point.x, water.GetHeight(hit.point) + 0.05f, hit.point.z);
                return true;
            }
        }

        target = default;
        water = null;
        return false;
    }

    private IEnumerator CastTimeoutRoutine()
    {
        yield return new WaitForSeconds(_castTimeout);
        _castTimeoutRoutine = null;

        if (_state == FishingState.Casting)
            BeginReturn(false);
    }

    private IEnumerator BiteWindowRoutine()
    {
        yield return new WaitForSeconds(_biteWindowDuration);
        _biteWindowRoutine = null;

        if (_state == FishingState.BiteWindow)
            BeginReturn(false);
    }

    private void StopFishingRoutines()
    {
        StopRoutine(ref _castTimeoutRoutine);
        StopRoutine(ref _biteWindowRoutine);
    }

    private void ResetRuntimeState()
    {
        _state = FishingState.Ready;
        _castTarget = default;
        _castWater = null;
        _rodReturned = false;
        _bobberReturned = false;
        _caughtFish = false;
    }

    private void StopRoutine(ref Coroutine routine)
    {
        if (routine == null)
            return;

        StopCoroutine(routine);
        routine = null;
    }

    private Transform FindChild(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}
