using System.Collections.Generic;
using FishAlive;
using UnityEngine;
using VContainer;

public sealed class AquariumController : MonoBehaviour
{
    [SerializeField] private AquariumSwimPoints _swimPoints;
    [SerializeField] private Transform _fishContainer;

    private readonly Dictionary<string, GameObject> _spawnedFish = new();
    private ProgressService _progress;

    [Inject]
    public void Construct(ProgressService progress)
    {
        _progress = progress;
    }

    private void Start()
    {
        if (_progress == null || _swimPoints == null || _swimPoints.Count == 0 || _fishContainer == null)
        {
            enabled = false;
            return;
        }

        _progress.NewSpeciesAdded += OnNewSpeciesAdded;
        _progress.ProgressReset += OnProgressReset;

        foreach (CaughtFish caughtFish in _progress.Collection)
            SpawnFish(caughtFish.Definition);
    }

    private void OnDestroy()
    {
        if (_progress != null)
        {
            _progress.NewSpeciesAdded -= OnNewSpeciesAdded;
            _progress.ProgressReset -= OnProgressReset;
        }
    }

    private void OnNewSpeciesAdded(CaughtFish caughtFish)
    {
        SpawnFish(caughtFish.Definition);
    }

    private void SpawnFish(FishDefinition definition)
    {
        if (definition.AquariumPrefab == null || _spawnedFish.ContainsKey(definition.Id))
            return;

        Transform spawnPoint = _swimPoints.GetRandomPoint(_fishContainer.position, null, 0f);

        if (spawnPoint == null)
            return;

        GameObject fish = Instantiate(definition.AquariumPrefab, spawnPoint.position, spawnPoint.rotation, _fishContainer);
        fish.name = definition.DisplayName;
        fish.transform.localScale = Vector3.one * definition.AquariumScale;
        ApplyRarityColor(fish, definition.Rarity);

        if (!fish.TryGetComponent(out FishMotion _))
        {
            Destroy(fish);
            return;
        }

        AquariumFishWander wander = fish.GetComponent<AquariumFishWander>();

        if (wander == null)
            wander = fish.AddComponent<AquariumFishWander>();

        wander.Initialize(_swimPoints);
        _spawnedFish.Add(definition.Id, fish);
    }

    private void OnProgressReset()
    {
        foreach (GameObject fish in _spawnedFish.Values)
        {
            if (fish != null)
                Destroy(fish);
        }

        _spawnedFish.Clear();
    }

    private static void ApplyRarityColor(GameObject fish, FishRarity rarity)
    {
        Color color = FishRarityColors.Get(rarity);
        MaterialPropertyBlock propertyBlock = new();

        foreach (Renderer renderer in fish.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer is ParticleSystemRenderer || renderer is LineRenderer)
                continue;

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
