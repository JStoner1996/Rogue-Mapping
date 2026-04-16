using System.Collections.Generic;

// Shared lookup helpers for equipment instance collections.
public static class EquipmentInstanceLookup
{
    public static EquipmentInstance FindById(IReadOnlyList<EquipmentInstance> availableEquipment, string equipmentId)
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
