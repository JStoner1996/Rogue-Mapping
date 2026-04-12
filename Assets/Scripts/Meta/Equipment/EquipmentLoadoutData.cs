using System;
using System.Collections.Generic;

[Serializable]
public class EquipmentLoadoutData
{
    public List<EquipmentLoadoutSlot> equippedItems = new List<EquipmentLoadoutSlot>();
}

[Serializable]
public class EquipmentLoadoutSlot
{
    public string slotId;
    public string equipmentInstanceId;
}
