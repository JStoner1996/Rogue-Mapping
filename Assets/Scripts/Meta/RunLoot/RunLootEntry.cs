using System;
using UnityEngine;

[Serializable]
public class RunLootEntry
{
    public string id;
    public RunLootType lootType;
    public string displayName;
    public Sprite icon;
    public bool isDiscarded;
    public MapInstance map;
    public EquipmentInstance equipment;
}
