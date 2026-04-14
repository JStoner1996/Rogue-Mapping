using System;
using System.Collections.Generic;

[Serializable]
public class OwnedEquipmentAffixRecord
{
    public string affixName;
    public EquipmentAffixType affixType;
    public List<EquipmentModifierRoll> modifierRolls = new List<EquipmentModifierRoll>();
}
