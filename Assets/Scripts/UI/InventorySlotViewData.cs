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
    public bool isDiscarded;
    public bool isInteractable = true;

    public static InventorySlotViewData Empty()
    {
        return new InventorySlotViewData
        {
            isEmpty = true,
            isInteractable = false,
        };
    }
}
