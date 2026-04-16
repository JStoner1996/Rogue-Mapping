using System.Collections.Generic;

// Syncs selected and hovered equipment state into the paper-doll slot visuals.
public static class EquipmentVisualStateSyncService
{
    public static void SyncEquippedSlotVisuals(
        IReadOnlyList<EquipmentSlotDropTargetUI> dropTargets,
        EquipmentInstance selectedEquipment,
        EquipmentInstance hoveredEquipment,
        IReadOnlyList<EquipmentInstance> availableEquipment,
        IEquipmentDataFacade dataFacade)
    {
        // Mirrors the shared equipment selection/hover state onto the paper-doll slot visuals.
        if (dropTargets == null || dataFacade == null)
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

            string equippedItemId = dataFacade.GetEquippedItemId(dropTarget.LoadoutSlotId);
            EquipmentInstance equippedItem = EquipmentInstanceLookup.FindById(availableEquipment, equippedItemId);

            dropTarget.SetDisplayedEquipment(equippedItem);
            dropTarget.SetSelected(equippedItem != null
                && selectedEquipment != null
                && equippedItem.InstanceId == selectedEquipment.InstanceId);
            dropTarget.SetHovered(equippedItem != null
                && hoveredEquipment != null
                && equippedItem.InstanceId == hoveredEquipment.InstanceId);
        }
    }
}
