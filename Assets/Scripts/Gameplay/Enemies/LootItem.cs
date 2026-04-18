using UnityEngine;

[System.Serializable]
public class LootItem
{
    public PowerUpLootType type;
    [Range(0, 100)] public float dropChance;

    public float GetAdjustedDropChance(float multiplier)
    {
        return dropChance * multiplier;
    }
}
