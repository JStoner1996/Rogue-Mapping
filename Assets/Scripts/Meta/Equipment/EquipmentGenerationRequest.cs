using System;
using System.Collections.Generic;

[Serializable]
public class EquipmentGenerationRequest
{
    public int minItemTier = 1;
    public int maxItemTier = 1;
    public int itemLevel;
    public bool forceSlotType;
    public EquipmentSlotType forcedSlotType = EquipmentSlotType.Head;
    public List<EquipmentStatType> requiredAffixStats = new List<EquipmentStatType>();

    public int GetClampedMinTier()
    {
        return Math.Max(1, minItemTier);
    }

    public int GetClampedMaxTier()
    {
        return Math.Max(GetClampedMinTier(), maxItemTier);
    }

    public int GetClampedItemLevel()
    {
        if (itemLevel > 0)
        {
            return itemLevel;
        }

        return 10 + ((GetClampedMaxTier() - 1) * 10);
    }
}
