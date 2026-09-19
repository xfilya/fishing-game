using System;
using UnityEngine;

[Serializable]
public sealed class FishDefinition
{
    [Header("Species")]
    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField] private FishRarity _rarity;
    [Header("Catch")]
    [SerializeField, Min(0.01f)] private float _minimumWeight = 0.1f;
    [SerializeField, Min(0.01f)] private float _maximumWeight = 1f;
    [Header("Aquarium")]
    [SerializeField] private GameObject _aquariumPrefab;
    [SerializeField, Min(0.01f)] private float _aquariumScale = 1f;

    public string Id => _id;
    public string DisplayName => _displayName;
    public FishRarity Rarity => _rarity;
    public float MinimumWeight => _minimumWeight;
    public float MaximumWeight => _maximumWeight;
    public GameObject AquariumPrefab => _aquariumPrefab;
    public float AquariumScale => _aquariumScale;

    public void EnsureValid()
    {
        _maximumWeight = Mathf.Max(_minimumWeight, _maximumWeight);
    }
}
