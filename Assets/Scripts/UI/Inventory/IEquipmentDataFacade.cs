using System.Collections.Generic;

// Provides the equipment inventory layer with the data it needs without binding it to a specific save system.
public interface IEquipmentDataFacade
{
    List<EquipmentInstance> GetOwnedEquipmentInstances();
    EquipmentLoadoutData GetEquipmentLoadout();
    string GetEquippedItemId(string loadoutSlotId);
    void SetEquippedItem(string loadoutSlotId, string equipmentInstanceId, bool saveImmediately = true);
    void Save();
    EquipmentStatSummary GetEquippedEquipmentStatSummary();
}
