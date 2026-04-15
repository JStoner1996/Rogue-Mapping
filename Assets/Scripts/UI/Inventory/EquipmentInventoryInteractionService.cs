using System.Collections.Generic;

// Handles equip, unequip, swap, and drop interaction rules for equipment items.
public static class EquipmentInventoryInteractionService
{
    public static bool ToggleEquipFromInventory(
        EquipmentInstance equipment,
        IReadOnlyList<EquipmentInstance> availableEquipment,
        IReadOnlyList<EquipmentSlotDropTargetUI> dropTargets,
        IEquipmentDataFacade dataFacade)
    {
        // Right-clicking from inventory either unequips the item if it's already worn,
        // or finds the best legal slot to equip it into.
        if (equipment == null || dataFacade == null)
        {
            return false;
        }

        EquipmentLoadoutData loadout = dataFacade.GetEquipmentLoadout();
        if (EquipmentInventoryLayoutService.IsEquipped(equipment.InstanceId, loadout))
        {
            return UnequipItem(equipment.InstanceId, dataFacade);
        }

        EquipmentSlotDropTargetUI targetSlot = EquipmentInventoryLayoutService.FindBestEquipTarget(
            equipment,
            dropTargets,
            availableEquipment,
            dataFacade.GetEquippedItemId);

        if (targetSlot == null)
        {
            return false;
        }

        dataFacade.SetEquippedItem(targetSlot.LoadoutSlotId, equipment.InstanceId);
        return true;
    }

    public static bool UnequipFromLoadoutSlot(string loadoutSlotId, IEquipmentDataFacade dataFacade)
    {
        if (string.IsNullOrWhiteSpace(loadoutSlotId) || dataFacade == null)
        {
            return false;
        }

        string equippedItemId = dataFacade.GetEquippedItemId(loadoutSlotId);
        if (string.IsNullOrWhiteSpace(equippedItemId))
        {
            return false;
        }

        dataFacade.SetEquippedItem(loadoutSlotId, string.Empty);
        return true;
    }

    public static bool HandleDropToEquipmentSlot(
        EquipmentSlotDropTargetUI dropTarget,
        DragItemPayload payload,
        IReadOnlyList<EquipmentInstance> availableEquipment,
        IEquipmentDataFacade dataFacade)
    {
        // Handles dragging onto the paper-doll, including same-type slot swaps like Ring 1 <-> Ring 2.
        if (dropTarget == null || payload == null || string.IsNullOrWhiteSpace(payload.itemId) || dataFacade == null)
        {
            return false;
        }

        EquipmentInstance equipment = FindEquipmentById(availableEquipment, payload.itemId);
        if (equipment == null || equipment.SlotType != dropTarget.SlotType)
        {
            return false;
        }

        if (payload.sourceType == DragItemSourceType.EquippedSlot
            && !string.IsNullOrWhiteSpace(payload.sourceSlotId)
            && payload.sourceSlotId != dropTarget.LoadoutSlotId)
        {
            string targetEquippedItemId = dataFacade.GetEquippedItemId(dropTarget.LoadoutSlotId);
            EquipmentInstance targetEquippedItem = FindEquipmentById(availableEquipment, targetEquippedItemId);

            if (targetEquippedItem != null && targetEquippedItem.SlotType == equipment.SlotType)
            {
                dataFacade.SetEquippedItem(payload.sourceSlotId, targetEquippedItem.InstanceId, false);
                dataFacade.SetEquippedItem(dropTarget.LoadoutSlotId, equipment.InstanceId, false);
                dataFacade.Save();
                return true;
            }
        }

        dataFacade.SetEquippedItem(dropTarget.LoadoutSlotId, equipment.InstanceId);
        return true;
    }

    public static bool CanAcceptInventoryDrop(
        int index,
        DragItemPayload payload,
        IReadOnlyList<string> equipmentInventoryLayout,
        IEquipmentDataFacade dataFacade)
    {
        if (payload == null
            || payload.itemType != DragItemType.Equipment
            || index < 0
            || dataFacade == null
            || equipmentInventoryLayout == null
            || index >= equipmentInventoryLayout.Count)
        {
            return false;
        }

        string targetItemId = equipmentInventoryLayout[index];
        return string.IsNullOrWhiteSpace(targetItemId)
            || !EquipmentInventoryLayoutService.IsEquipped(targetItemId, dataFacade.GetEquipmentLoadout());
    }

    public static bool HandleInventorySlotDrop(
        int targetIndex,
        DragItemPayload payload,
        List<string> equipmentInventoryLayout,
        IReadOnlyList<EquipmentInstance> availableEquipment,
        IEquipmentDataFacade dataFacade)
    {
        // Handles inventory-slot drops, including the special case where dropping an equipped item
        // onto a same-type inventory item equips that target item into the dragged item's old slot.
        if (!CanAcceptInventoryDrop(targetIndex, payload, equipmentInventoryLayout, dataFacade)
            || string.IsNullOrWhiteSpace(payload.itemId))
        {
            return false;
        }

        int sourceIndex = equipmentInventoryLayout.FindIndex(id => id == payload.itemId);
        string targetItemId = targetIndex >= 0 && targetIndex < equipmentInventoryLayout.Count
            ? equipmentInventoryLayout[targetIndex]
            : string.Empty;
        EquipmentInstance targetEquipment = FindEquipmentById(availableEquipment, targetItemId);

        if (payload.sourceType == DragItemSourceType.EquippedSlot
            && targetEquipment != null
            && !string.IsNullOrWhiteSpace(payload.sourceSlotId)
            && targetEquipment.SlotType == payload.equipmentSlotType
            && targetEquipment.InstanceId != payload.itemId)
        {
            dataFacade.SetEquippedItem(payload.sourceSlotId, targetEquipment.InstanceId);

            if (sourceIndex >= 0)
            {
                equipmentInventoryLayout[sourceIndex] = targetEquipment.InstanceId;
            }

            equipmentInventoryLayout[targetIndex] = payload.itemId;
            return true;
        }

        if (sourceIndex < 0)
        {
            return false;
        }

        if (payload.sourceType == DragItemSourceType.EquippedSlot)
        {
            UnequipItem(payload.itemId, dataFacade);
        }

        if (sourceIndex != targetIndex)
        {
            equipmentInventoryLayout[targetIndex] = equipmentInventoryLayout[sourceIndex];
            equipmentInventoryLayout[sourceIndex] = targetItemId;
        }

        return true;
    }

    public static bool UnequipItem(string equipmentInstanceId, IEquipmentDataFacade dataFacade)
    {
        // Removes an item from any loadout slot currently pointing at it.
        if (string.IsNullOrWhiteSpace(equipmentInstanceId) || dataFacade == null)
        {
            return false;
        }

        EquipmentLoadoutData loadout = dataFacade.GetEquipmentLoadout();
        bool changed = false;

        if (loadout?.equippedItems != null)
        {
            for (int i = 0; i < loadout.equippedItems.Count; i++)
            {
                EquipmentLoadoutSlot slot = loadout.equippedItems[i];

                if (slot != null && slot.equipmentInstanceId == equipmentInstanceId)
                {
                    dataFacade.SetEquippedItem(slot.slotId, string.Empty, false);
                    changed = true;
                }
            }
        }

        if (changed)
        {
            dataFacade.Save();
        }

        return changed;
    }

    private static EquipmentInstance FindEquipmentById(IReadOnlyList<EquipmentInstance> availableEquipment, string equipmentId)
    {
        if (availableEquipment == null || string.IsNullOrWhiteSpace(equipmentId))
        {
            return null;
        }

        for (int i = 0; i < availableEquipment.Count; i++)
        {
            EquipmentInstance item = availableEquipment[i];
            if (item != null && item.InstanceId == equipmentId)
            {
                return item;
            }
        }

        return null;
    }
}
