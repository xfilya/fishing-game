using UnityEngine;

public sealed class AudioSurface : MonoBehaviour
{
    [SerializeField] private bool _isStone;

    public bool IsStone => _isStone;
}
