using System.Collections.Generic;

// Adapts MetaProgressionService to the reusable equipment inventory data facade.
public class MetaProgressionEquipmentDataFacade : IEquipmentDataFacade
{
    public List<EquipmentInstance> GetOwnedEquipmentInstances()
    {
        return MetaProgressionService.GetOwnedEquipmentInstances();
    }

    public EquipmentLoadoutData GetEquipmentLoadout()
    {
        return MetaProgressionService.GetEquipmentLoadout();
    }

    public string GetEquippedItemId(string loadoutSlotId)
    {
        return MetaProgressionService.GetEquippedItemId(loadoutSlotId);
    }

    public void SetEquippedItem(string loadoutSlotId, string equipmentInstanceId, bool saveImmediately = true)
    {
        MetaProgressionService.SetEquippedItem(loadoutSlotId, equipmentInstanceId, saveImmediately);
    }

    public void Save()
    {
        MetaProgressionService.Save();
    }

    public EquipmentStatSummary GetEquippedEquipmentStatSummary()
    {
        return MetaProgressionService.GetEquippedEquipmentStatSummary();
    }
}
