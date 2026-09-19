using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public sealed class FishShadowSpawner : MonoBehaviour
{
    [SerializeField] private FishingWater _water;
    [SerializeField] private Transform _islandCenter;
    [SerializeField] private FishShadow _shadowPrefab;
    [SerializeField, Min(1)] private int _shadowCount = 9;
    [SerializeField, Min(0f)] private float _minimumSpawnRadius = 37f;
    [SerializeField, Min(0.1f)] private float _maximumSpawnRadius = 46f;
    [SerializeField, Min(0.1f)] private float _roamingRadius = 6f;
    [SerializeField, Range(0f, 30f)] private float _spawnAngleJitter = 7f;
    [SerializeField, Min(0f)] private float _minimumSpacing = 3f;
    [SerializeField, Min(0.1f)] private float _landCheckRadius = 0.65f;
    [SerializeField, Min(0f)] private float _minimumRespawnDelay = 3f;
    [SerializeField, Min(0f)] private float _maximumRespawnDelay = 6f;

    private readonly List<FishShadow> _shadows = new();
    private readonly Collider[] _landCheckResults = new Collider[16];

    private void Start()
    {
        if (_water == null || _islandCenter == null || _shadowPrefab == null)
        {
            enabled = false;
            return;
        }

        for (int i = 0; i < _shadowCount; i++)
            SpawnShadow(i);
    }

    public bool TryAttractClosest(Bobber bobber, float attractionRadius, Action<FishShadow> reachedTarget, out FishShadow attractedShadow)
    {
        attractedShadow = null;
        float closestDistanceSqr = attractionRadius * attractionRadius;

        foreach (FishShadow shadow in _shadows)
        {
            if (!shadow.IsAvailable)
                continue;

            float distanceSqr = HorizontalDistanceSqr(shadow.transform.position, bobber.transform.position);

            if (distanceSqr > closestDistanceSqr)
                continue;

            closestDistanceSqr = distanceSqr;
            attractedShadow = shadow;
        }

        if (attractedShadow == null)
            return false;

        attractedShadow.Attract(bobber, reachedTarget);
        return true;
    }

    public bool IsRoamingPositionValid(Vector3 position, Vector3 homePosition)
    {
        if (HorizontalDistanceSqr(position, homePosition) > _roamingRadius * _roamingRadius)
            return false;

        return IsWaterPositionValid(position);
    }

    public Vector3 GetDirectionToHome(Vector3 position, Vector3 homePosition)
    {
        Vector3 direction = homePosition - position;
        direction.y = 0f;
        return direction.sqrMagnitude > Mathf.Epsilon ? direction.normalized : Vector3.forward;
    }

    public void NotifyConsumed(FishShadow shadow)
    {
        StartCoroutine(RespawnRoutine(shadow));
    }

    private bool IsWaterPositionValid(Vector3 position)
    {

        float waterHeight = _water.GetHeight(position);
        Vector3 surfaceProbeOrigin = new(position.x, waterHeight + 30f, position.z);

        if (!Physics.Raycast(surfaceProbeOrigin, Vector3.down, out RaycastHit surfaceHit, 60f, ~0, QueryTriggerInteraction.Collide) || surfaceHit.collider.GetComponentInParent<FishingWater>() == null)
            return false;

        int count = Physics.OverlapSphereNonAlloc(position + Vector3.up * 0.4f, _landCheckRadius, _landCheckResults, ~0, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider candidate = _landCheckResults[i];

            if (candidate == null || candidate.GetComponentInParent<FishingWater>() != null)
                continue;

            return false;
        }

        return true;
    }

    private void SpawnShadow(int index)
    {
        Vector3 position = FindSpawnPosition(index);
        FishShadow shadow = Instantiate(_shadowPrefab, position, Quaternion.identity, transform);
        shadow.name = $"FishShadow {_shadows.Count + 1}";
        shadow.Initialize(this, _water, position);
        _shadows.Add(shadow);
    }

    private IEnumerator RespawnRoutine(FishShadow shadow)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(_minimumRespawnDelay, _maximumRespawnDelay));
        shadow.ResetRoaming(FindSpawnPosition(Mathf.Max(0, _shadows.IndexOf(shadow))));
    }

    private Vector3 FindSpawnPosition(int index)
    {
        float sectorAngle = 360f * index / Mathf.Max(1, _shadowCount);
        Vector3 fallbackDirection = Quaternion.Euler(0f, sectorAngle, 0f) * Vector3.forward;
        Vector3 fallback = _islandCenter.position + fallbackDirection * _maximumSpawnRadius;
        fallback.y = _water.GetHeight(fallback) + 0.12f;

        for (int attempt = 0; attempt < 40; attempt++)
        {
            float angle = sectorAngle + UnityEngine.Random.Range(-_spawnAngleJitter, _spawnAngleJitter);
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Vector3 position = _islandCenter.position + direction * UnityEngine.Random.Range(_minimumSpawnRadius, _maximumSpawnRadius);
            position.y = _water.GetHeight(position) + 0.12f;
            fallback = position;

            if (!IsWaterPositionValid(position) || IsTooCloseToShadow(position))
                continue;

            return position;
        }

        return fallback;
    }

    private bool IsTooCloseToShadow(Vector3 position)
    {
        float minimumDistanceSqr = _minimumSpacing * _minimumSpacing;

        foreach (FishShadow shadow in _shadows)
        {
            if (shadow.gameObject.activeInHierarchy && HorizontalDistanceSqr(position, shadow.transform.position) < minimumDistanceSqr)
                return true;
        }

        return false;
    }

    private static float HorizontalDistanceSqr(Vector3 first, Vector3 second)
    {
        float x = first.x - second.x;
        float z = first.z - second.z;
        return x * x + z * z;
    }

    private void OnValidate()
    {
        _maximumSpawnRadius = Mathf.Max(_minimumSpawnRadius + 0.1f, _maximumSpawnRadius);
        _maximumRespawnDelay = Mathf.Max(_minimumRespawnDelay, _maximumRespawnDelay);
    }
}
