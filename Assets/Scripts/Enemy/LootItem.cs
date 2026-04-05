using UnityEngine;

[System.Serializable]
public class LootItem

{
    public LootType type;
    public GameObject itemPrefab;
    [Range(0, 100)] public float dropChance;
}
