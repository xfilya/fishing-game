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
    [SerializeField] private ParticleSystem _biteEffectPrefab;
    [SerializeField] private Vector3 _biteEffectOffset = new(0f, 0.18f, 0f);
    [SerializeField] private LayerMask _castMask = ~0;
    [SerializeField, Min(1f)] private float _maxCastDistance = 30f;
    [SerializeField, Min(0.1f)] private float _flightDuration = 0.7f;
    [SerializeField, Min(0.1f)] private float _reelSpeed = 15f;
    [SerializeField, Min(0.1f)] private float _castTimeout = 3f;
    [SerializeField, Min(0.1f)] private float _biteWindowDuration = 1f;
    [SerializeField, Min(0.1f)] private float _shadowAttractionRadius = 6f;
    [SerializeField, Min(0f)] private float _minimumBiteDelay = 0.65f;
    [SerializeField, Min(0f)] private float _maximumBiteDelay = 1.4f;

    private IInputService _input;
    private CatchGenerator _catchGenerator;
    private ProgressService _progress;
    private FishShadowSpawner _shadowSpawner;
    private FishingState _state = FishingState.Ready;
    private Bobber _bobber;
    private FishShadow _attractedShadow;
    private Vector3 _castTarget;
    private FishingWater _castWater;
    private ParticleSystem _biteEffect;
    private Coroutine _castTimeoutRoutine;
    private Coroutine _biteDelayRoutine;
    private Coroutine _biteWindowRoutine;
    private bool _rodReturned;
    private bool _bobberReturned;
    private bool _caughtFish;

    public event Action CastBegan;
    public event Action BeginReturned;
    public event Action CatchCompleted;
    public event Action<CaughtFish> CatchResolved;
    public event Action ResultClosed;

    public FishingState State => _state;

    [Inject]
    public void Construct(IInputService inputService, CatchGenerator catchGenerator, ProgressService progress, FishShadowSpawner shadowSpawner)
    {
        _input = inputService;
        _catchGenerator = catchGenerator;
        _progress = progress;
        _shadowSpawner = shadowSpawner;
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
        StopBiteEffect();
        ReleaseAttractedShadow();

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
        if (_input != null && _catchGenerator != null && _progress != null && _shadowSpawner != null)
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

    private void LateUpdate()
    {
        if (_biteEffect != null && _bobber != null)
            _biteEffect.transform.position = GetBiteEffectPosition();
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

        CastBegan?.Invoke();
        Debug.Log($"[FishingController] Начало заброса: цель {_castTarget} в воде {_castWater.name}");
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

        TryAttractShadow();
    }

    private void TryAttractShadow()
    {
        if (_bobber != null && _shadowSpawner.TryAttractClosest(_bobber, _shadowAttractionRadius, OnShadowReachedBobber, out _attractedShadow))
            Debug.Log($"[FishingController] Тень рыбы заметила поплавок на расстоянии до {_shadowAttractionRadius:0.0} м");
        else
            Debug.Log("[FishingController] Рядом с поплавком нет тени рыбы, поклёвки не будет");
    }

    [ContextMenu("Test Bite")]
    public void NotifyBiteStarted()
    {
        if (_state != FishingState.Waiting)
            return;

        ConsumeAttractedShadow();
        _state = FishingState.BiteWindow;
        _bobber.Dip();
        StartBiteEffect();
        _biteWindowRoutine = StartCoroutine(BiteWindowRoutine());
    }

    private void BeginReturn(bool caughtFish)
    {
        if (_state == FishingState.Reeling || _state == FishingState.Ready || _state == FishingState.Result)
            return;

        StopFishingRoutines();
        StopBiteEffect();

        if (!caughtFish)
            ReleaseAttractedShadow();

        _caughtFish = caughtFish;
        _state = FishingState.Reeling;
        _rodView.PlayReturn();

        if (_bobber != null)
            _bobber.BeginReel(_reelSpeed);
        else
            _bobberReturned = true;

        BeginReturned?.Invoke();
        Debug.Log($"[FishingController] Начало возврата: {(caughtFish ? "поймана рыба" : "рыба не поймана")}");
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
            CaughtFish caughtFish = _catchGenerator.Generate();
            _progress.RegisterCatch(caughtFish);
            _state = FishingState.Result;
            CatchCompleted?.Invoke();
            CatchResolved?.Invoke(caughtFish);
            return;
        }

        _state = FishingState.Ready;
    }

    public void CompleteResult()
    {
        if (_state != FishingState.Result)
            return;

        _state = FishingState.Ready;
        ResultClosed?.Invoke();
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
            CompleteMissedBite();
    }

    private void CompleteMissedBite()
    {
        StopBiteEffect();

        if (_bobber != null)
            _bobber.ReleaseBite();

        _state = FishingState.Waiting;
        TryAttractShadow();
        Debug.Log("[FishingController] Рыба сорвалась, поплавок возвращается на поверхность");
    }

    private void StopFishingRoutines()
    {
        StopRoutine(ref _castTimeoutRoutine);
        StopRoutine(ref _biteDelayRoutine);
        StopRoutine(ref _biteWindowRoutine);
    }

    private void OnShadowReachedBobber(FishShadow shadow)
    {
        if (_state != FishingState.Waiting || shadow != _attractedShadow)
        {
            shadow.Release();
            return;
        }

        StopRoutine(ref _biteDelayRoutine);
        _biteDelayRoutine = StartCoroutine(BiteDelayRoutine());
    }

    private IEnumerator BiteDelayRoutine()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(_minimumBiteDelay, _maximumBiteDelay));
        _biteDelayRoutine = null;

        if (_state == FishingState.Waiting)
            NotifyBiteStarted();
    }

    private void ReleaseAttractedShadow()
    {
        if (_attractedShadow == null)
            return;

        _attractedShadow.Release();
        _attractedShadow = null;
    }

    private void ConsumeAttractedShadow()
    {
        if (_attractedShadow == null)
            return;

        _attractedShadow.Consume();
        _attractedShadow = null;
    }

    private void StartBiteEffect()
    {
        StopBiteEffect();

        if (_biteEffectPrefab == null || _bobber == null)
            return;

        _biteEffect = Instantiate(_biteEffectPrefab, GetBiteEffectPosition(), _biteEffectPrefab.transform.rotation);

        foreach (ParticleSystem system in _biteEffect.GetComponentsInChildren<ParticleSystem>(true))
        {
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            system.Play(true);
        }
    }

    private void StopBiteEffect()
    {
        if (_biteEffect == null)
            return;

        Destroy(_biteEffect.gameObject);
        _biteEffect = null;
    }

    private Vector3 GetBiteEffectPosition()
    {
        Vector3 position = _bobber.transform.position;

        if (_castWater != null)
            position.y = _castWater.GetHeight(position);

        return position + _biteEffectOffset;
    }

    private void ResetRuntimeState()
    {
        _state = FishingState.Ready;
        _castTarget = default;
        _castWater = null;
        _attractedShadow = null;
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

    private void OnValidate()
    {
        _maximumBiteDelay = Mathf.Max(_minimumBiteDelay, _maximumBiteDelay);
    }
}
