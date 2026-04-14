using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EquipmentRolledAffix
{
    [SerializeField] private EquipmentAffixDefinition affixDefinition;
    [SerializeField] private List<EquipmentModifierRoll> modifierRolls = new List<EquipmentModifierRoll>();

    public EquipmentAffixDefinition AffixDefinition => affixDefinition;
    public string AffixName => affixDefinition != null ? affixDefinition.AffixName : string.Empty;
    public int AffixTier => affixDefinition != null ? affixDefinition.AffixTier : 0;
    public EquipmentAffixType AffixType => affixDefinition != null ? affixDefinition.AffixType : default;
    public IReadOnlyList<EquipmentModifierRoll> ModifierRolls => modifierRolls;

    public EquipmentRolledAffix(EquipmentAffixDefinition affixDefinition, List<EquipmentModifierRoll> modifierRolls)
    {
        this.affixDefinition = affixDefinition;
        this.modifierRolls = modifierRolls ?? new List<EquipmentModifierRoll>();
    }
}
