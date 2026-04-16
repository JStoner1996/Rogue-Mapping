using System.Collections.Generic;
using UnityEngine;

// Builds the inventory grid view model for equipment items.
public static class EquipmentInventoryGridPresenter
{
    public static InventoryGridModel BuildGridModel(
        int maxSlots,
        IReadOnlyList<string> equipmentInventoryLayout,
        IReadOnlyList<EquipmentInstance> availableEquipment,
        EquipmentInstance selectedEquipment,
        EquipmentInstance hoveredEquipment,
        IEquipmentDataFacade dataFacade)
    {
        // Converts the current equipment inventory layout into generic slot models for the reusable grid UI.
        int slotCount = Mathf.Max(maxSlots, equipmentInventoryLayout != null ? equipmentInventoryLayout.Count : 0);
        List<InventorySlotModel> items = new List<InventorySlotModel>(slotCount);
        EquipmentLoadoutData loadout = dataFacade != null ? dataFacade.GetEquipmentLoadout() : null;

        for (int i = 0; i < slotCount; i++)
        {
            string equipmentId = equipmentInventoryLayout != null && i < equipmentInventoryLayout.Count
                ? equipmentInventoryLayout[i]
                : string.Empty;
            EquipmentInstance equipment = EquipmentInstanceLookup.FindById(availableEquipment, equipmentId);

            if (equipment == null)
            {
                items.Add(InventorySlotModel.Empty());
                continue;
            }

            bool isEquipped = EquipmentInventoryLayoutService.IsEquipped(equipment.InstanceId, loadout);

            items.Add(new InventorySlotModel
            {
                id = equipment.InstanceId,
                label = equipment.DisplayName,
                icon = equipment.Icon,
                iconTint = GetEquipmentTierTint(equipment.ItemTier),
                isEmpty = false,
                isSelected = equipment == selectedEquipment,
                isHovered = equipment == hoveredEquipment,
                isEquipped = isEquipped,
                isInteractable = true,
                canDrag = !isEquipped,
                dragItemType = DragItemType.Equipment,
                dragItemSourceType = DragItemSourceType.Inventory,
                hasEquipmentSlotType = true,
                equipmentSlotType = equipment.SlotType,
            });
        }

        return new InventoryGridModel(items, maxSlots);
    }

    private static Color GetEquipmentTierTint(int itemTier)
    {
        float normalizedTier = Mathf.InverseLerp(1f, 10f, Mathf.Clamp(itemTier, 1, 10));
        return Color.Lerp(Color.white, Color.red, normalizedTier);
    }
}
