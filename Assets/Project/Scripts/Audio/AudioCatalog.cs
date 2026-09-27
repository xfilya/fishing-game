using UnityEngine;

[CreateAssetMenu(fileName = "AudioCatalog", menuName = "Fishing/Audio Catalog")]
public sealed class AudioCatalog : ScriptableObject
{
    [Header("Movement")]
    public AudioClip[] SandSteps;
    public AudioClip[] StoneSteps;
    public AudioClip[] WoodSteps;
    public AudioClip Jump;
    public AudioClip SandLanding;
    public AudioClip StoneLanding;
    public AudioClip WoodLanding;

    [Header("Fishing")]
    public AudioClip CastWindup;
    public AudioClip CastRelease;
    public AudioClip BobberSplash;
    public AudioClip FishBite;
    public AudioClip BiteBubbles;
    public AudioClip Reel;
    public AudioClip Catch;

    [Header("Interface")]
    public AudioClip Click;
    public AudioClip Coins;

    [Header("Atmosphere")]
    public AudioClip Ocean;
    public AudioClip Birds;
    public AudioClip Music;
    public AudioClip AquariumBubbles;
}
