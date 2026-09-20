using UnityEngine;

public static class FishRarityColors
{
    public static Color Get(FishRarity rarity)
    {
        return rarity switch
        {
            FishRarity.Common => new Color(0.78f, 0.9f, 1f, 1f),
            FishRarity.Rare => new Color(0.18f, 0.65f, 1f, 1f),
            FishRarity.Epic => new Color(0.72f, 0.3f, 1f, 1f),
            FishRarity.Legendary => new Color(1f, 0.62f, 0.08f, 1f),
            _ => Color.white
        };
    }
}
