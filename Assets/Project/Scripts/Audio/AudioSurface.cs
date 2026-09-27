using UnityEngine;

public enum AudioSurfaceType
{
    Sand,
    Stone,
    Wood
}

public sealed class AudioSurface : MonoBehaviour
{
    [SerializeField] private AudioSurfaceType _surfaceType;

    public AudioSurfaceType SurfaceType => _surfaceType;
}
