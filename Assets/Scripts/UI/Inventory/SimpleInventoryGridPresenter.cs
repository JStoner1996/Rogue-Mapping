using System;
using System.Collections.Generic;
using UnityEngine;

// Builds simple inventory grids for list-style tabs like weapons and maps.
public static class SimpleInventoryGridPresenter
{
    public static InventoryGridModel BuildGridModel<T>(
        IReadOnlyList<T> items,
        int maxSlots,
        T selectedItem,
        T hoveredItem,
        Func<T, string> getId,
        Func<T, string> getLabel,
        Func<T, Sprite> getIcon)
        where T : class
    {
        List<InventorySlotModel> slotModels = new List<InventorySlotModel>(items != null ? items.Count : 0);

        if (items != null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                T item = items[i];
                if (item == null)
                {
                    continue;
                }

                slotModels.Add(new InventorySlotModel
                {
                    id = getId(item),
                    label = getLabel(item),
                    icon = getIcon(item),
                    isEmpty = false,
                    isSelected = EqualityComparer<T>.Default.Equals(item, selectedItem),
                    isHovered = EqualityComparer<T>.Default.Equals(item, hoveredItem),
                    isInteractable = true,
                });
            }
        }

        return new InventoryGridModel(slotModels, maxSlots);
    }
}
