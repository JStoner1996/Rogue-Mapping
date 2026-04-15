using System.Collections.Generic;

public static class EquipmentInventoryInteractionService
{
    public static bool ToggleEquipFromInventory(
        EquipmentInstance equipment,
        IReadOnlyList<EquipmentInstance> availableEquipment,
        IReadOnlyList<EquipmentSlotDropTargetUI> dropTargets)
    {
        if (equipment == null)
        {
            return false;
        }

        EquipmentLoadoutData loadout = MetaProgressionService.GetEquipmentLoadout();
        if (EquipmentInventoryLayoutService.IsEquipped(equipment.InstanceId, loadout))
        {
            return UnequipItem(equipment.InstanceId);
        }

        EquipmentSlotDropTargetUI targetSlot = EquipmentInventoryLayoutService.FindBestEquipTarget(
            equipment,
            dropTargets,
            availableEquipment,
            MetaProgressionService.GetEquippedItemId);

        if (targetSlot == null)
        {
            return false;
        }

        MetaProgressionService.SetEquippedItem(targetSlot.LoadoutSlotId, equipment.InstanceId);
        return true;
    }

    public static bool UnequipFromLoadoutSlot(string loadoutSlotId)
    {
        if (string.IsNullOrWhiteSpace(loadoutSlotId))
        {
            return false;
        }

        string equippedItemId = MetaProgressionService.GetEquippedItemId(loadoutSlotId);
        if (string.IsNullOrWhiteSpace(equippedItemId))
        {
            return false;
        }

        MetaProgressionService.SetEquippedItem(loadoutSlotId, string.Empty);
        return true;
    }

    public static bool HandleDropToEquipmentSlot(
        EquipmentSlotDropTargetUI dropTarget,
        DragItemPayload payload,
        IReadOnlyList<EquipmentInstance> availableEquipment)
    {
        if (dropTarget == null || payload == null || string.IsNullOrWhiteSpace(payload.itemId))
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
            string targetEquippedItemId = MetaProgressionService.GetEquippedItemId(dropTarget.LoadoutSlotId);
            EquipmentInstance targetEquippedItem = FindEquipmentById(availableEquipment, targetEquippedItemId);

            if (targetEquippedItem != null && targetEquippedItem.SlotType == equipment.SlotType)
            {
                MetaProgressionService.SetEquippedItem(payload.sourceSlotId, targetEquippedItem.InstanceId, false);
                MetaProgressionService.SetEquippedItem(dropTarget.LoadoutSlotId, equipment.InstanceId, false);
                MetaProgressionService.Save();
                return true;
            }
        }

        MetaProgressionService.SetEquippedItem(dropTarget.LoadoutSlotId, equipment.InstanceId);
        return true;
    }

    public static bool CanAcceptInventoryDrop(
        int index,
        DragItemPayload payload,
        IReadOnlyList<string> equipmentInventoryLayout)
    {
        if (payload == null
            || payload.itemType != DragItemType.Equipment
            || index < 0
            || equipmentInventoryLayout == null
            || index >= equipmentInventoryLayout.Count)
        {
            return false;
        }

        string targetItemId = equipmentInventoryLayout[index];
        return string.IsNullOrWhiteSpace(targetItemId)
            || !EquipmentInventoryLayoutService.IsEquipped(targetItemId, MetaProgressionService.GetEquipmentLoadout());
    }

    public static bool HandleInventorySlotDrop(
        int targetIndex,
        DragItemPayload payload,
        List<string> equipmentInventoryLayout,
        IReadOnlyList<EquipmentInstance> availableEquipment)
    {
        if (!CanAcceptInventoryDrop(targetIndex, payload, equipmentInventoryLayout)
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
            MetaProgressionService.SetEquippedItem(payload.sourceSlotId, targetEquipment.InstanceId);

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
            UnequipItem(payload.itemId);
        }

        if (sourceIndex != targetIndex)
        {
            equipmentInventoryLayout[targetIndex] = equipmentInventoryLayout[sourceIndex];
            equipmentInventoryLayout[sourceIndex] = targetItemId;
        }

        return true;
    }

    public static bool UnequipItem(string equipmentInstanceId)
    {
        if (string.IsNullOrWhiteSpace(equipmentInstanceId))
        {
            return false;
        }

        EquipmentLoadoutData loadout = MetaProgressionService.GetEquipmentLoadout();
        bool changed = false;

        if (loadout?.equippedItems != null)
        {
            for (int i = 0; i < loadout.equippedItems.Count; i++)
            {
                EquipmentLoadoutSlot slot = loadout.equippedItems[i];

                if (slot != null && slot.equipmentInstanceId == equipmentInstanceId)
                {
                    MetaProgressionService.SetEquippedItem(slot.slotId, string.Empty, false);
                    changed = true;
                }
            }
        }

        if (changed)
        {
            MetaProgressionService.Save();
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
