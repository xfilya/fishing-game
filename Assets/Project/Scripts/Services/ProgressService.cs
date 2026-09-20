using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ProgressService
{
    private readonly Dictionary<string, CaughtFish> _collection = new();
    private readonly List<CaughtFish> _inventory = new();
    private readonly SaveService _saveService;

    public event Action<CaughtFish> NewSpeciesAdded;
    public event Action InventoryChanged;
    public event Action ProgressReset;

    public IReadOnlyCollection<CaughtFish> Collection => _collection.Values;
    public IReadOnlyList<CaughtFish> Inventory => _inventory;

    public ProgressService(FishCatalog catalog, SaveService saveService)
    {
        _saveService = saveService;
        Load(catalog);
    }

    public bool ContainsSpecies(string speciesId)
    {
        return _collection.ContainsKey(speciesId);
    }

    public bool RegisterCatch(CaughtFish caughtFish)
    {
        if (_collection.TryAdd(caughtFish.Definition.Id, caughtFish))
        {
            caughtFish.MarkAsNewSpecies();
            Save();
            NewSpeciesAdded?.Invoke(caughtFish);
            return true;
        }

        CaughtFish record = _collection[caughtFish.Definition.Id];

        if (caughtFish.Weight > record.Weight)
        {
            caughtFish.MarkAsNewRecord();
            _collection[caughtFish.Definition.Id] = caughtFish;
            _inventory.Add(record);
        }
        else
        {
            _inventory.Add(caughtFish);
        }

        Save();
        InventoryChanged?.Invoke();
        return false;
    }

    public bool RemoveFromInventory(CaughtFish caughtFish)
    {
        if (!_inventory.Remove(caughtFish))
            return false;

        Save();
        InventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveAllFromInventory()
    {
        if (_inventory.Count == 0)
            return false;

        _inventory.Clear();
        Save();
        InventoryChanged?.Invoke();
        return true;
    }

    public void ResetProgress()
    {
        _collection.Clear();
        _inventory.Clear();
        Save();
        InventoryChanged?.Invoke();
        ProgressReset?.Invoke();
    }

    private void Load(FishCatalog catalog)
    {
        Dictionary<string, FishDefinition> definitions = new();

        foreach (FishDefinition definition in catalog.Fish)
            definitions[definition.Id] = definition;

        foreach (SavedFishData savedFish in _saveService.Data.Collection)
        {
            if (savedFish != null && definitions.TryGetValue(savedFish.SpeciesId, out FishDefinition definition) && !_collection.ContainsKey(savedFish.SpeciesId))
                _collection.Add(savedFish.SpeciesId, new CaughtFish(definition, Mathf.Clamp(savedFish.Weight, definition.MinimumWeight, definition.MaximumWeight)));
        }

        foreach (SavedFishData savedFish in _saveService.Data.Inventory)
        {
            if (savedFish != null && definitions.TryGetValue(savedFish.SpeciesId, out FishDefinition definition) && _collection.ContainsKey(savedFish.SpeciesId))
                _inventory.Add(new CaughtFish(definition, Mathf.Clamp(savedFish.Weight, definition.MinimumWeight, definition.MaximumWeight)));
        }
    }

    private void Save()
    {
        GameSaveData data = _saveService.Data;
        data.Collection.Clear();
        data.Inventory.Clear();

        foreach (CaughtFish caughtFish in _collection.Values)
            data.Collection.Add(new SavedFishData(caughtFish.Definition.Id, caughtFish.Weight));

        foreach (CaughtFish caughtFish in _inventory)
            data.Inventory.Add(new SavedFishData(caughtFish.Definition.Id, caughtFish.Weight));

        _saveService.Save();
    }
}
