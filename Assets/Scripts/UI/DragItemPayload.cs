using System;
using UnityEngine;

[Serializable]
public class DragItemPayload
{
    public string itemId;
    public string label;
    public Sprite icon;
    public DragItemType itemType;
    public bool hasEquipmentSlotType;
    public EquipmentSlotType equipmentSlotType;

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(itemId) && itemType != DragItemType.Unknown;
    }
}
