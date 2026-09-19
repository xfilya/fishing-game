using System;
using System.Collections.Generic;

public sealed class ProgressService
{
    private readonly Dictionary<string, CaughtFish> _collection = new();
    private readonly List<CaughtFish> _inventory = new();

    public event Action<CaughtFish> NewSpeciesAdded;
    public event Action InventoryChanged;

    public IReadOnlyCollection<CaughtFish> Collection => _collection.Values;
    public IReadOnlyList<CaughtFish> Inventory => _inventory;

    public bool ContainsSpecies(string speciesId)
    {
        return _collection.ContainsKey(speciesId);
    }

    public bool RegisterCatch(CaughtFish caughtFish)
    {
        if (_collection.TryAdd(caughtFish.Definition.Id, caughtFish))
        {
            caughtFish.MarkAsNewSpecies();
            NewSpeciesAdded?.Invoke(caughtFish);
            return true;
        }

        _inventory.Add(caughtFish);
        InventoryChanged?.Invoke();
        return false;
    }
}
