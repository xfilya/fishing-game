using StylizedWater3;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class FishingWater : MonoBehaviour
{
    [SerializeField] private Transform _surface;
    [SerializeField] private float _heightOffset;
    [SerializeField] private WaterObject _waterObject;
    [SerializeField] private WaveProfile _waveProfile;

    private Collider _collider;
    private HeightQuerySystem.Sampler _heightSampler;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _heightSampler = new HeightQuerySystem.Sampler();
        _heightSampler.SetSampleCount(1, true);
    }

    public float GetHeight(Vector3 position)
    {
        if (_waterObject != null && _waterObject.material != null && _waveProfile != null)
        {
            _heightSampler.SetSamplePosition(0, position);
            Gerstner.ComputeHeight(_heightSampler, _waveProfile, _waterObject.transform.position.y, _waterObject.material);
            return _heightSampler.heightValues[0] + _heightOffset;
        }

        if (_surface != null)
            return _surface.position.y + _heightOffset;

        if (_waterObject != null)
            return _waterObject.transform.position.y + _heightOffset;

        return _collider.bounds.max.y + _heightOffset;
    }

    private void OnDestroy()
    {
        _heightSampler?.Dispose();
    }
}
