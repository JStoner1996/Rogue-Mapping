using System;
using UnityEngine;

[Serializable]
public class InventorySlotViewData
{
    public string id;
    public string label;
    public Sprite icon;
    public bool isEmpty;
    public bool isSelected;
    public bool isFocused;
    public bool isDiscarded;
    public bool isInteractable = true;
    public DragItemType dragItemType;
    public bool hasEquipmentSlotType;
    public EquipmentSlotType equipmentSlotType;

    public static InventorySlotViewData Empty()
    {
        return new InventorySlotViewData
        {
            isEmpty = true,
            isInteractable = false,
            dragItemType = DragItemType.Unknown,
        };
    }
}
