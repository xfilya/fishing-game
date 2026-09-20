using System.Collections.Generic;
using UnityEngine;

public sealed class CatchGenerator
{
    private readonly FishCatalog _catalog;
    private readonly ProgressService _progress;
    private readonly EconomyService _economy;
    private readonly List<FishDefinition> _rarityCandidates = new();

    public CatchGenerator(FishCatalog catalog, ProgressService progress, EconomyService economy)
    {
        _catalog = catalog;
        _progress = progress;
        _economy = economy;
    }

    public CaughtFish Generate()
    {
        FishRarity rarity = RollRarity();
        FishDefinition definition = RollDefinition(rarity);
        float normalizedWeight = Mathf.Pow(Random.value, 1f / Mathf.Max(1f, _economy.WeightBias));
        float weight = Mathf.Lerp(definition.MinimumWeight, definition.MaximumWeight, normalizedWeight);
        return new CaughtFish(definition, weight);
    }

    private FishRarity RollRarity()
    {
        float totalChance = 0f;

        foreach (FishRarity rarity in System.Enum.GetValues(typeof(FishRarity)))
            totalChance += GetModifiedRarityChance(rarity);

        float roll = Random.Range(0f, totalChance);
        float accumulatedChance = 0f;

        foreach (FishRarity rarity in System.Enum.GetValues(typeof(FishRarity)))
        {
            accumulatedChance += GetModifiedRarityChance(rarity);

            if (roll <= accumulatedChance)
                return rarity;
        }

        return FishRarity.Common;
    }

    private float GetModifiedRarityChance(FishRarity rarity)
    {
        float luck = _economy.TotalRarityLuck;
        float multiplier = rarity switch
        {
            FishRarity.Common => Mathf.Max(0.2f, 1f - luck * 0.02f),
            FishRarity.Rare => 1f + luck * 0.025f,
            FishRarity.Epic => 1f + luck * 0.04f,
            FishRarity.Legendary => 1f + luck * 0.055f,
            _ => 1f
        };
        return _catalog.GetRarityChance(rarity) * multiplier;
    }

    private FishDefinition RollDefinition(FishRarity rarity)
    {
        _rarityCandidates.Clear();

        foreach (FishDefinition definition in _catalog.Fish)
        {
            if (definition.Rarity == rarity)
                _rarityCandidates.Add(definition);
        }

        if (_rarityCandidates.Count == 0)
            _rarityCandidates.AddRange(_catalog.Fish);

        float totalWeight = 0f;

        foreach (FishDefinition definition in _rarityCandidates)
            totalWeight += _progress.ContainsSpecies(definition.Id) ? 1f : _catalog.NewSpeciesWeightMultiplier;

        float roll = Random.Range(0f, totalWeight);

        foreach (FishDefinition definition in _rarityCandidates)
        {
            roll -= _progress.ContainsSpecies(definition.Id) ? 1f : _catalog.NewSpeciesWeightMultiplier;

            if (roll <= 0f)
                return definition;
        }

        return _rarityCandidates[^1];
    }
}
