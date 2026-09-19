using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FishCatalog", menuName = "Fishing/Fish Catalog")]
public sealed class FishCatalog : ScriptableObject
{
    [Header("Species")]
    [SerializeField] private List<FishDefinition> _fish = new();
    [Header("Rarity Chances")]
    [SerializeField, Min(0f)] private float _commonChance = 60f;
    [SerializeField, Min(0f)] private float _rareChance = 25f;
    [SerializeField, Min(0f)] private float _epicChance = 11f;
    [SerializeField, Min(0f)] private float _legendaryChance = 4f;
    [SerializeField, Min(1f)] private float _newSpeciesWeightMultiplier = 2.5f;

    public IReadOnlyList<FishDefinition> Fish => _fish;
    public float NewSpeciesWeightMultiplier => _newSpeciesWeightMultiplier;

    public float GetRarityChance(FishRarity rarity)
    {
        return rarity switch
        {
            FishRarity.Common => _commonChance,
            FishRarity.Rare => _rareChance,
            FishRarity.Epic => _epicChance,
            FishRarity.Legendary => _legendaryChance,
            _ => 0f
        };
    }

    private void OnValidate()
    {
        foreach (FishDefinition definition in _fish)
            definition?.EnsureValid();
    }
}
