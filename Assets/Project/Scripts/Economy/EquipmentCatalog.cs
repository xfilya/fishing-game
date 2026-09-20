using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentCatalog", menuName = "Fishing/Equipment Catalog")]
public sealed class EquipmentCatalog : ScriptableObject
{
    [SerializeField] private List<EquipmentDefinition> _rods = new();
    [SerializeField] private List<EquipmentDefinition> _bobbers = new();

    public IReadOnlyList<EquipmentDefinition> Rods => _rods;
    public IReadOnlyList<EquipmentDefinition> Bobbers => _bobbers;

    public EquipmentDefinition Get(string id)
    {
        foreach (EquipmentDefinition item in _rods)
        {
            if (item.Id == id)
                return item;
        }

        foreach (EquipmentDefinition item in _bobbers)
        {
            if (item.Id == id)
                return item;
        }

        return null;
    }

    public EquipmentDefinition GetStarter(EquipmentType type)
    {
        IReadOnlyList<EquipmentDefinition> items = GetItems(type);
        return items.Count > 0 ? items[0] : null;
    }

    public EquipmentDefinition GetPrevious(EquipmentDefinition item)
    {
        IReadOnlyList<EquipmentDefinition> items = GetItems(item.Type);

        for (int i = 1; i < items.Count; i++)
        {
            if (items[i].Id == item.Id)
                return items[i - 1];
        }

        return null;
    }

    public IReadOnlyList<EquipmentDefinition> GetItems(EquipmentType type)
    {
        return type == EquipmentType.Rod ? _rods : _bobbers;
    }

    public void ResetToDefaults()
    {
        _rods = new List<EquipmentDefinition>
        {
            new("rod_blue", "Синяя удочка", EquipmentType.Rod, 0, 0, 0f, 1f, new Color(0.12f, 0.48f, 1f)),
            new("rod_green", "Зелёная удочка", EquipmentType.Rod, 1, 180, 2f, 1.2f, new Color(0.2f, 0.85f, 0.42f)),
            new("rod_red", "Красная удочка", EquipmentType.Rod, 2, 550, 5f, 1.55f, new Color(1f, 0.2f, 0.16f)),
            new("rod_orange", "Оранжевая удочка", EquipmentType.Rod, 3, 1400, 9f, 2f, new Color(1f, 0.5f, 0.08f)),
            new("rod_black", "Чёрная удочка", EquipmentType.Rod, 4, 3600, 15f, 2.8f, new Color(0.025f, 0.035f, 0.055f))
        };
        _bobbers = new List<EquipmentDefinition>
        {
            new("bobber_red", "Красный поплавок", EquipmentType.Bobber, 0, 0, 0f, 1f, new Color(1f, 0.08f, 0.08f)),
            new("bobber_green", "Зелёный поплавок", EquipmentType.Bobber, 1, 120, 4f, 1f, new Color(0.18f, 0.9f, 0.4f)),
            new("bobber_blue", "Синий поплавок", EquipmentType.Bobber, 2, 350, 8f, 1f, new Color(0.1f, 0.55f, 1f)),
            new("bobber_orange", "Оранжевый поплавок", EquipmentType.Bobber, 3, 900, 14f, 1f, new Color(1f, 0.5f, 0.06f)),
            new("bobber_black", "Чёрный поплавок", EquipmentType.Bobber, 4, 2400, 22f, 1f, new Color(0.025f, 0.035f, 0.055f))
        };
    }

    private void OnValidate()
    {
        _rods.Sort((first, second) => first.Tier.CompareTo(second.Tier));
        _bobbers.Sort((first, second) => first.Tier.CompareTo(second.Tier));
    }
}
