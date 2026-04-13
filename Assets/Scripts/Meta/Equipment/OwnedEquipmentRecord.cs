using System;
using System.Collections.Generic;

[Serializable]
public class OwnedEquipmentRecord
{
    public string instanceId;
    public EquipmentRarity rarity;
    public int itemTier;
    public string baseName;
    public string prefixAffixName;
    public string suffixAffixName;
    public string slotId;
    public List<EquipmentModifierRoll> implicitRolls = new List<EquipmentModifierRoll>();
    public List<EquipmentModifierRoll> prefixRolls = new List<EquipmentModifierRoll>();
    public List<EquipmentModifierRoll> suffixRolls = new List<EquipmentModifierRoll>();
}
