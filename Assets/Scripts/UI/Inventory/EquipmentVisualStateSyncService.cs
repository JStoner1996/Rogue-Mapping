using System.Collections.Generic;

public static class EquipmentVisualStateSyncService
{
    public static void SyncEquippedSlotVisuals(
        IReadOnlyList<EquipmentSlotDropTargetUI> dropTargets,
        EquipmentInstance selectedEquipment,
        EquipmentInstance hoveredEquipment,
        IReadOnlyList<EquipmentInstance> availableEquipment)
    {
        if (dropTargets == null)
        {
            return;
        }

        for (int i = 0; i < dropTargets.Count; i++)
        {
            EquipmentSlotDropTargetUI dropTarget = dropTargets[i];
            if (dropTarget == null)
            {
                continue;
            }

            string equippedItemId = MetaProgressionService.GetEquippedItemId(dropTarget.LoadoutSlotId);
            EquipmentInstance equippedItem = FindEquipmentById(availableEquipment, equippedItemId);

            dropTarget.SetDisplayedEquipment(equippedItem);
            dropTarget.SetSelected(equippedItem != null
                && selectedEquipment != null
                && equippedItem.InstanceId == selectedEquipment.InstanceId);
            dropTarget.SetHovered(equippedItem != null
                && hoveredEquipment != null
                && equippedItem.InstanceId == hoveredEquipment.InstanceId);
        }
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
