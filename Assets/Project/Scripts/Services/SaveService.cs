using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SaveService
{
    private const string SaveKey = "FishOfMyDreams.Save.v1";

    public GameSaveData Data { get; private set; }

    public SaveService()
    {
        Load();
    }

    public void Save()
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }

    public void Reset()
    {
        Data = new GameSaveData();
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Data = new GameSaveData();
            return;
        }

        try
        {
            Data = JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString(SaveKey)) ?? new GameSaveData();
            Data.EnsureValid();
        }
        catch
        {
            Data = new GameSaveData();
        }
    }
}

[Serializable]
public sealed class GameSaveData
{
    public int Coins;
    public string EquippedRodId;
    public string EquippedBobberId;
    public List<string> OwnedEquipmentIds = new();
    public List<SavedFishData> Collection = new();
    public List<SavedFishData> Inventory = new();

    public void EnsureValid()
    {
        Coins = Mathf.Max(0, Coins);
        OwnedEquipmentIds ??= new List<string>();
        Collection ??= new List<SavedFishData>();
        Inventory ??= new List<SavedFishData>();
    }
}

[Serializable]
public sealed class SavedFishData
{
    public string SpeciesId;
    public float Weight;

    public SavedFishData(string speciesId, float weight)
    {
        SpeciesId = speciesId;
        Weight = weight;
    }
}
