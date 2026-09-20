using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EconomyService
{
    private readonly EquipmentCatalog _catalog;
    private readonly ProgressService _progress;
    private readonly SaveService _saveService;
    private readonly HashSet<string> _ownedEquipment = new();
    private int _coins;
    private EquipmentDefinition _equippedRod;
    private EquipmentDefinition _equippedBobber;

    public event Action<int> BalanceChanged;
    public event Action<EquipmentDefinition> EquipmentChanged;

    public int Coins => _coins;
    public EquipmentDefinition EquippedRod => _equippedRod;
    public EquipmentDefinition EquippedBobber => _equippedBobber;
    public float TotalRarityLuck => (_equippedRod?.RarityLuck ?? 0f) + (_equippedBobber?.RarityLuck ?? 0f);
    public float WeightBias => _equippedRod?.WeightBias ?? 1f;

    public EconomyService(EquipmentCatalog catalog, ProgressService progress, SaveService saveService)
    {
        _catalog = catalog;
        _progress = progress;
        _saveService = saveService;
        Load();
    }

    public bool IsOwned(string itemId)
    {
        return _ownedEquipment.Contains(itemId);
    }

    public bool IsEquipped(string itemId)
    {
        return _equippedRod?.Id == itemId || _equippedBobber?.Id == itemId;
    }

    public bool CanPurchase(EquipmentDefinition item)
    {
        if (item == null || IsOwned(item.Id) || _coins < item.Price)
            return false;

        EquipmentDefinition previous = _catalog.GetPrevious(item);
        return previous == null || IsOwned(previous.Id);
    }

    public bool IsPreviousTierOwned(EquipmentDefinition item)
    {
        EquipmentDefinition previous = _catalog.GetPrevious(item);
        return previous == null || IsOwned(previous.Id);
    }

    public bool TryPurchase(EquipmentDefinition item)
    {
        if (!CanPurchase(item))
            return false;

        _coins -= item.Price;
        _ownedEquipment.Add(item.Id);
        EquipInternal(item);
        Save();
        BalanceChanged?.Invoke(_coins);
        EquipmentChanged?.Invoke(item);
        return true;
    }

    public bool TryEquip(EquipmentDefinition item)
    {
        if (item == null || !IsOwned(item.Id))
            return false;

        EquipInternal(item);
        Save();
        EquipmentChanged?.Invoke(item);
        return true;
    }

    public int GetFishPrice(CaughtFish fish)
    {
        if (fish?.Definition == null)
            return 0;

        float minimum = fish.Definition.MinimumWeight;
        float maximum = Mathf.Max(minimum, fish.Definition.MaximumWeight);
        float normalizedWeight = Mathf.InverseLerp(minimum, maximum, fish.Weight);
        float rarityBase = fish.Definition.Rarity switch
        {
            FishRarity.Common => 20f,
            FishRarity.Rare => 55f,
            FishRarity.Epic => 160f,
            FishRarity.Legendary => 450f,
            _ => 20f
        };
        float valuePerKilogram = fish.Definition.Rarity switch
        {
            FishRarity.Common => 45f,
            FishRarity.Rare => 65f,
            FishRarity.Epic => 85f,
            FishRarity.Legendary => 110f,
            _ => 45f
        };
        float weightMultiplier = Mathf.Lerp(0.8f, 1.25f, normalizedWeight);
        return Mathf.Max(1, Mathf.RoundToInt((rarityBase + fish.Weight * valuePerKilogram) * weightMultiplier));
    }

    public bool TrySell(CaughtFish fish, out int earnedCoins)
    {
        earnedCoins = GetFishPrice(fish);

        if (earnedCoins <= 0 || !_progress.RemoveFromInventory(fish))
        {
            earnedCoins = 0;
            return false;
        }

        AddCoins(earnedCoins);
        return true;
    }

    public int SellAll()
    {
        CaughtFish[] fish = new CaughtFish[_progress.Inventory.Count];

        for (int i = 0; i < fish.Length; i++)
            fish[i] = _progress.Inventory[i];

        int earnedCoins = 0;

        foreach (CaughtFish caughtFish in fish)
            earnedCoins += GetFishPrice(caughtFish);

        if (fish.Length == 0 || !_progress.RemoveAllFromInventory())
            return 0;

        AddCoins(earnedCoins);
        return earnedCoins;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        _coins = (int)Math.Min(int.MaxValue, (long)_coins + amount);
        Save();
        BalanceChanged?.Invoke(_coins);
    }

    public void RemoveCoins(int amount)
    {
        if (amount <= 0)
            return;

        SetCoins(_coins - amount);
    }

    public void SetCoins(int amount)
    {
        _coins = Mathf.Max(0, amount);
        Save();
        BalanceChanged?.Invoke(_coins);
    }

    public void ResetAllProgress()
    {
        _saveService.Reset();
        _progress.ResetProgress();
        _coins = 0;
        _ownedEquipment.Clear();
        EquipmentDefinition starterRod = _catalog.GetStarter(EquipmentType.Rod);
        EquipmentDefinition starterBobber = _catalog.GetStarter(EquipmentType.Bobber);

        if (starterRod != null)
            _ownedEquipment.Add(starterRod.Id);

        if (starterBobber != null)
            _ownedEquipment.Add(starterBobber.Id);

        _equippedRod = starterRod;
        _equippedBobber = starterBobber;
        Save();
        BalanceChanged?.Invoke(_coins);
        EquipmentChanged?.Invoke(_equippedRod);
        EquipmentChanged?.Invoke(_equippedBobber);
    }

    private void Load()
    {
        GameSaveData data = _saveService.Data;
        _coins = Mathf.Max(0, data.Coins);

        foreach (string itemId in data.OwnedEquipmentIds)
        {
            if (_catalog.Get(itemId) != null)
                _ownedEquipment.Add(itemId);
        }

        EquipmentDefinition starterRod = _catalog.GetStarter(EquipmentType.Rod);
        EquipmentDefinition starterBobber = _catalog.GetStarter(EquipmentType.Bobber);

        if (starterRod != null)
            _ownedEquipment.Add(starterRod.Id);

        if (starterBobber != null)
            _ownedEquipment.Add(starterBobber.Id);

        _equippedRod = GetOwnedOrStarter(data.EquippedRodId, starterRod);
        _equippedBobber = GetOwnedOrStarter(data.EquippedBobberId, starterBobber);
        Save();
    }

    private EquipmentDefinition GetOwnedOrStarter(string itemId, EquipmentDefinition starter)
    {
        EquipmentDefinition item = _catalog.Get(itemId);
        return item != null && IsOwned(item.Id) ? item : starter;
    }

    private void EquipInternal(EquipmentDefinition item)
    {
        if (item.Type == EquipmentType.Rod)
            _equippedRod = item;
        else
            _equippedBobber = item;
    }

    private void Save()
    {
        GameSaveData data = _saveService.Data;
        data.Coins = _coins;
        data.OwnedEquipmentIds.Clear();
        data.OwnedEquipmentIds.AddRange(_ownedEquipment);
        data.EquippedRodId = _equippedRod?.Id;
        data.EquippedBobberId = _equippedBobber?.Id;
        _saveService.Save();
    }
}
