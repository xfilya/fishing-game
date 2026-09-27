using UnityEngine;

public sealed class AudioService : MonoBehaviour
{
    [SerializeField] private AudioCatalog _catalog;
    [SerializeField] private Transform _musicOrigin;
    [SerializeField] private Transform _aquariumOrigin;
    [SerializeField, Range(0f, 1f)] private float _defaultMasterVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float _defaultMusicVolume = 0.55f;
    [SerializeField, Range(0f, 1f)] private float _defaultAmbienceVolume = 0.38f;
    [SerializeField, Range(0f, 1f)] private float _defaultEffectsVolume = 0.8f;
    [SerializeField, Range(0f, 2f)] private float _musicGain = 1.75f;
    [SerializeField, Range(0f, 1f)] private float _castWindupVolume = 0.35f;
    [SerializeField, Range(0f, 1f)] private float _castReleaseVolume = 0.35f;

    private AudioSource _music;
    private AudioSource _ocean;
    private AudioSource _birds;
    private AudioSource _aquarium;
    private AudioSource _effects;
    private AudioSource _reel;
    private AudioSource _interface;
    private AudioSource _bubbles;
    private AudioSource[] _worldSources;
    private int _nextWorldSource;

    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float AmbienceVolume { get; private set; }
    public float EffectsVolume { get; private set; }

    private void Awake()
    {
        MasterVolume = PlayerPrefs.GetFloat("Audio.Master", _defaultMasterVolume);
        MusicVolume = PlayerPrefs.GetFloat("Audio.Music", _defaultMusicVolume);
        AmbienceVolume = PlayerPrefs.GetFloat("Audio.Ambience", _defaultAmbienceVolume);
        EffectsVolume = PlayerPrefs.GetFloat("Audio.Effects", _defaultEffectsVolume);

        _music = CreateSource("Music", _musicOrigin != null);
        _music.minDistance = 18f;
        _music.maxDistance = 75f;

        if (_musicOrigin != null)
            _music.transform.position = _musicOrigin.position;

        _ocean = CreateSource("Ocean", false);
        _birds = CreateSource("Birds", false);
        _aquarium = CreateSource("Aquarium", true);
        _aquarium.minDistance = 3f;
        _aquarium.maxDistance = 22f;

        if (_aquariumOrigin != null)
            _aquarium.transform.position = _aquariumOrigin.position;

        _effects = CreateSource("Effects", false);
        _reel = CreateSource("Reel", false);
        _interface = CreateSource("Interface", false);
        _bubbles = CreateSource("Bite Bubbles", false);
        _worldSources = new AudioSource[4];

        for (int i = 0; i < _worldSources.Length; i++)
            _worldSources[i] = CreateSource($"World Effect {i + 1}", true);

        ApplyVolumes();
    }

    private void Start()
    {
        StartLoop(_music, _catalog?.Music);
        StartLoop(_ocean, _catalog?.Ocean);
        StartLoop(_birds, _catalog?.Birds);

        if (_aquariumOrigin != null)
            StartLoop(_aquarium, _catalog?.AquariumBubbles);
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("Audio.Master", MasterVolume);
        ApplyVolumes();
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("Audio.Music", MusicVolume);
        ApplyVolumes();
    }

    public void SetAmbienceVolume(float value)
    {
        AmbienceVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("Audio.Ambience", AmbienceVolume);
        ApplyVolumes();
    }

    public void SetEffectsVolume(float value)
    {
        EffectsVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("Audio.Effects", EffectsVolume);
        ApplyVolumes();
    }

    public void PlayStep(AudioSurfaceType surface)
    {
        AudioClip[] clips = surface switch
        {
            AudioSurfaceType.Stone => _catalog?.StoneSteps,
            AudioSurfaceType.Wood => _catalog?.WoodSteps,
            _ => _catalog?.SandSteps
        };

        if (clips == null || clips.Length == 0)
            return;

        _effects.pitch = Random.Range(0.94f, 1.06f);
        _effects.PlayOneShot(clips[Random.Range(0, clips.Length)], 0.55f);
    }

    public void PlayJump() => PlayEffect(_catalog?.Jump, 0.5f);
    public void PlayLanding(AudioSurfaceType surface) => PlayEffect(surface switch
    {
        AudioSurfaceType.Stone => _catalog?.StoneLanding,
        AudioSurfaceType.Wood => _catalog?.WoodLanding,
        _ => _catalog?.SandLanding
    }, 0.9f);
    public void PlayCastWindup() => PlayEffect(_catalog?.CastWindup, _castWindupVolume);
    public void PlayCastRelease() => PlayEffect(_catalog?.CastRelease, _castReleaseVolume);
    public void PlayCatch() => PlayEffect(_catalog?.Catch, 0.85f);
    public void PlayClick() => PlayInterface(_catalog?.Click, 0.5f);
    public void PlayCoins() => PlayInterface(_catalog?.Coins, 0.85f);
    public void PlayBobberSplash(Vector3 position) => PlayWorld(_catalog?.BobberSplash, position, 0.95f);
    public void PlayFishBite(Vector3 position) => PlayWorld(_catalog?.FishBite, position, 1f);

    public void StartBiteBubbles()
    {
        if (_catalog?.BiteBubbles == null)
            return;

        StartLoop(_bubbles, _catalog.BiteBubbles);
    }

    public void StopBiteBubbles()
    {
        if (_bubbles != null)
            _bubbles.Stop();
    }

    public void StartReel()
    {
        if (_catalog?.Reel != null)
        {
            _reel.clip = _catalog.Reel;
            _reel.Play();
        }
    }

    public void StopReel()
    {
        if (_reel != null)
            _reel.Stop();
    }

    private void PlayEffect(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        _effects.pitch = 1f;
        _effects.PlayOneShot(clip, volume);
    }

    private void PlayInterface(AudioClip clip, float volume)
    {
        if (clip != null)
            _interface.PlayOneShot(clip, volume);
    }

    private void PlayWorld(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null)
            return;

        AudioSource source = _worldSources[_nextWorldSource];
        _nextWorldSource = (_nextWorldSource + 1) % _worldSources.Length;
        source.transform.position = position;
        source.pitch = Random.Range(0.96f, 1.04f);
        source.PlayOneShot(clip, volume);
    }

    private AudioSource CreateSource(string sourceName, bool spatial)
    {
        GameObject child = new(sourceName);
        child.transform.SetParent(transform, false);
        AudioSource source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = spatial ? 1f : 0f;
        source.minDistance = 3f;
        source.maxDistance = 32f;
        source.rolloffMode = AudioRolloffMode.Linear;
        return source;
    }

    private void StartLoop(AudioSource source, AudioClip clip)
    {
        if (clip == null)
            return;

        source.clip = clip;
        source.loop = true;
        source.Play();
    }

    private void ApplyVolumes()
    {
        float master = MasterVolume;
        _music.volume = Mathf.Clamp01(master * MusicVolume * _musicGain);
        _ocean.volume = master * AmbienceVolume * 0.5f;
        _birds.volume = master * AmbienceVolume * 0.45f;
        _aquarium.volume = master * AmbienceVolume * 0.5f;
        _effects.volume = master * EffectsVolume;
        _reel.volume = master * EffectsVolume * 0.7f;
        _interface.volume = master * EffectsVolume;
        _bubbles.volume = master * EffectsVolume * 0.35f;

        foreach (AudioSource source in _worldSources)
            source.volume = master * EffectsVolume;
    }

    private void OnApplicationQuit() => PlayerPrefs.Save();
}
