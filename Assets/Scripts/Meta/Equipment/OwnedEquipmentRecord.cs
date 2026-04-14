using System;
using System.Collections.Generic;

[Serializable]
public class OwnedEquipmentRecord
{
    public string instanceId;
    public EquipmentRarity rarity;
    public int itemTier;
    public string baseName;
    public List<OwnedEquipmentAffixRecord> prefixAffixes = new List<OwnedEquipmentAffixRecord>();
    public List<OwnedEquipmentAffixRecord> suffixAffixes = new List<OwnedEquipmentAffixRecord>();

    // Legacy save fields retained for migration from older one-prefix/one-suffix records.
    public string prefixAffixName;
    public string suffixAffixName;
    public string slotId;
    public List<EquipmentModifierRoll> implicitRolls = new List<EquipmentModifierRoll>();
    public List<EquipmentModifierRoll> prefixRolls = new List<EquipmentModifierRoll>();
    public List<EquipmentModifierRoll> suffixRolls = new List<EquipmentModifierRoll>();
}
