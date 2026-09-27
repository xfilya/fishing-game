using UnityEngine;
using VContainer;

public sealed class FishingAudio : MonoBehaviour
{
    private AudioService _audio;
    private FishingController _fishing;
    private RodView _rod;

    [Inject]
    public void Construct(AudioService audio) => _audio = audio;

    private void Awake()
    {
        _fishing = GetComponent<FishingController>();
        _rod = GetComponentInChildren<RodView>(true);
    }

    private void OnEnable()
    {
        _fishing.CastBegan += OnCastBegan;
        _fishing.BobberSplashed += OnBobberSplashed;
        _fishing.BiteStarted += OnBiteStarted;
        _fishing.BiteEnded += OnBiteEnded;
        _fishing.BeginReturned += OnBeginReturned;
        _fishing.ReturnCompleted += OnReturnCompleted;
        _fishing.CatchCompleted += OnCatchCompleted;

        if (_rod != null)
            _rod.CastReleased += OnCastReleased;
    }

    private void OnDisable()
    {
        _fishing.CastBegan -= OnCastBegan;
        _fishing.BobberSplashed -= OnBobberSplashed;
        _fishing.BiteStarted -= OnBiteStarted;
        _fishing.BiteEnded -= OnBiteEnded;
        _fishing.BeginReturned -= OnBeginReturned;
        _fishing.ReturnCompleted -= OnReturnCompleted;
        _fishing.CatchCompleted -= OnCatchCompleted;

        if (_rod != null)
            _rod.CastReleased -= OnCastReleased;

        _audio?.StopBiteBubbles();
        _audio?.StopReel();
    }

    private void OnCastBegan() => _audio?.PlayCastWindup();
    private void OnCastReleased() => _audio?.PlayCastRelease();
    private void OnBobberSplashed(Vector3 position) => _audio?.PlayBobberSplash(position);

    private void OnBiteStarted(Vector3 position)
    {
        _audio?.PlayFishBite(position);
        _audio?.StartBiteBubbles();
    }

    private void OnBiteEnded() => _audio?.StopBiteBubbles();

    private void OnBeginReturned()
    {
        _audio?.StopBiteBubbles();
        _audio?.StartReel();
    }

    private void OnReturnCompleted() => _audio?.StopReel();
    private void OnCatchCompleted() => _audio?.PlayCatch();
}
