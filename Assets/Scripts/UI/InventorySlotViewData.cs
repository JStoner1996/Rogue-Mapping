using System;
using UnityEngine;

[Serializable]
public class InventorySlotViewData
{
    public string id;
    public string label;
    public Sprite icon;
    public Color iconTint = Color.white;
    public bool isEmpty;
    public bool isSelected;
    public bool isFocused;
    public bool isDiscarded;
    public bool isEquipped;
    public bool isInteractable = true;
    public bool canDrag = true;
    public DragItemType dragItemType;
    public DragItemSourceType dragItemSourceType;
    public bool hasEquipmentSlotType;
    public EquipmentSlotType equipmentSlotType;

    public static InventorySlotViewData Empty()
    {
        return new InventorySlotViewData
        {
            isEmpty = true,
            isInteractable = false,
            canDrag = false,
            dragItemType = DragItemType.Unknown,
            dragItemSourceType = DragItemSourceType.Unknown,
        };
    }
}
