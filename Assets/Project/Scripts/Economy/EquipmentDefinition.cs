using System;
using UnityEngine;

[Serializable]
public sealed class EquipmentDefinition
{
    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField] private EquipmentType _type;
    [SerializeField, Min(0)] private int _tier;
    [SerializeField, Min(0)] private int _price;
    [SerializeField, Min(0f)] private float _rarityLuck;
    [SerializeField, Min(1f)] private float _weightBias = 1f;
    [SerializeField] private Color _color = Color.white;

    public string Id => _id;
    public string DisplayName => _displayName;
    public EquipmentType Type => _type;
    public int Tier => _tier;
    public int Price => _price;
    public float RarityLuck => _rarityLuck;
    public float WeightBias => _weightBias;
    public Color Color => _color;

    public EquipmentDefinition(string id, string displayName, EquipmentType type, int tier, int price, float rarityLuck, float weightBias, Color color)
    {
        _id = id;
        _displayName = displayName;
        _type = type;
        _tier = tier;
        _price = price;
        _rarityLuck = rarityLuck;
        _weightBias = weightBias;
        _color = color;
    }
}
