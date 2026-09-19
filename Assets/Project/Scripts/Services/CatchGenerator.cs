using System.Collections.Generic;
using UnityEngine;

public sealed class CatchGenerator
{
    private readonly FishCatalog _catalog;
    private readonly ProgressService _progress;
    private readonly List<FishDefinition> _rarityCandidates = new();

    public CatchGenerator(FishCatalog catalog, ProgressService progress)
    {
        _catalog = catalog;
        _progress = progress;
    }

    public CaughtFish Generate()
    {
        FishRarity rarity = RollRarity();
        FishDefinition definition = RollDefinition(rarity);
        float weight = Random.Range(definition.MinimumWeight, definition.MaximumWeight);
        return new CaughtFish(definition, weight);
    }

    private FishRarity RollRarity()
    {
        float totalChance = 0f;

        foreach (FishRarity rarity in System.Enum.GetValues(typeof(FishRarity)))
            totalChance += _catalog.GetRarityChance(rarity);

        float roll = Random.Range(0f, totalChance);
        float accumulatedChance = 0f;

        foreach (FishRarity rarity in System.Enum.GetValues(typeof(FishRarity)))
        {
            accumulatedChance += _catalog.GetRarityChance(rarity);

            if (roll <= accumulatedChance)
                return rarity;
        }

        return FishRarity.Common;
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
