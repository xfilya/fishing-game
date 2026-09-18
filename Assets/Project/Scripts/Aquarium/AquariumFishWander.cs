using FishAlive;
using UnityEngine;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(FishMotion))]
public sealed class AquariumFishWander : MonoBehaviour
{
    [SerializeField] private AquariumSwimPoints _swimPoints;
    [SerializeField, Min(0f)] private float _minimumTargetDistance = 0.5f;
    [SerializeField, Min(0f)] private float _minimumPauseDuration = 0.2f;
    [SerializeField, Min(0f)] private float _maximumPauseDuration = 1.2f;

    private FishMotion _fishMotion;
    private Transform _currentPoint;
    private float _nextMoveTime;
    private bool _isWaiting;

    private void Awake()
    {
        _fishMotion = GetComponent<FishMotion>();
    }

    public void Initialize(AquariumSwimPoints swimPoints)
    {
        _swimPoints = swimPoints;
    }

    private void Start()
    {
        if (_swimPoints == null || _swimPoints.Count == 0)
        {
            Debug.LogError("[AquariumFishWander] Assign a non-empty AquariumSwimPoints", this);
            enabled = false;
            return;
        }

        if (_swimPoints.transform.IsChildOf(transform))
        {
            Debug.LogError("[AquariumFishWander] Swim points cannot be children of the fish", this);
            enabled = false;
            return;
        }

        _fishMotion.BiteAtReach = false;
        _fishMotion.SetAutoMotion(true);
        SelectNextPoint();
    }

    private void Update()
    {
        if (_fishMotion.State != SwimState.Stopped)
            return;

        if (!_isWaiting)
        {
            _isWaiting = true;
            _nextMoveTime = Time.time + Random.Range(_minimumPauseDuration, _maximumPauseDuration);
            return;
        }

        if (Time.time >= _nextMoveTime)
            SelectNextPoint();
    }

    private void SelectNextPoint()
    {
        Transform nextPoint = _swimPoints.GetRandomPoint(transform.position, _currentPoint, _minimumTargetDistance);

        if (nextPoint == null)
        {
            enabled = false;
            return;
        }

        _currentPoint = nextPoint;
        _isWaiting = false;
        _fishMotion.target = _currentPoint.gameObject;
        _fishMotion.BiteAtReach = false;
        _fishMotion.SetReachMode(ReachMode.Wander);
        _fishMotion.PingTarget();
        _fishMotion.SetReachMode(ReachMode.Position);
    }

    private void OnValidate()
    {
        _maximumPauseDuration = Mathf.Max(_minimumPauseDuration, _maximumPauseDuration);
    }
}
