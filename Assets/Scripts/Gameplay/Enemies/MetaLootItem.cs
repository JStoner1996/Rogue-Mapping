using UnityEngine;

[System.Serializable]
public class MetaLootItem
{
    public MetaLootType type;
    [Range(0, 100)] public float dropChance;
    public MapDropSettings mapDropSettings = new MapDropSettings();
    public EquipmentDropSettings equipmentDropSettings = new EquipmentDropSettings();

    public float GetAdjustedDropChance(float multiplier)
    {
        return dropChance * multiplier;
    }
}
