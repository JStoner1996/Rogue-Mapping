using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EquipmentInstance
{
    [SerializeField] private string instanceId;
    [SerializeField] private EquipmentRarity rarity;
    [SerializeField] private int itemTier;
    [SerializeField] private EquipmentBaseDefinition baseDefinition;
    [SerializeField] private EquipmentAffixDefinition prefixAffix;
    [SerializeField] private EquipmentAffixDefinition suffixAffix;
    [SerializeField] private List<EquipmentModifierRoll> implicitRolls = new List<EquipmentModifierRoll>();
    [SerializeField] private List<EquipmentModifierRoll> prefixRolls = new List<EquipmentModifierRoll>();
    [SerializeField] private List<EquipmentModifierRoll> suffixRolls = new List<EquipmentModifierRoll>();

    public string InstanceId => instanceId;
    public EquipmentRarity Rarity => rarity;
    public int ItemTier => itemTier;
    public EquipmentBaseDefinition BaseDefinition => baseDefinition;
    public EquipmentAffixDefinition PrefixAffix => prefixAffix;
    public EquipmentAffixDefinition SuffixAffix => suffixAffix;
    public IReadOnlyList<EquipmentModifierRoll> ImplicitRolls => implicitRolls;
    public IReadOnlyList<EquipmentModifierRoll> PrefixRolls => prefixRolls;
    public IReadOnlyList<EquipmentModifierRoll> SuffixRolls => suffixRolls;

    public EquipmentSlotType SlotType => baseDefinition != null ? baseDefinition.SlotType : default;
    public string BaseName => baseDefinition != null ? baseDefinition.BaseName : string.Empty;
    public Sprite Icon => EquipmentIconResolver.ResolveIcon(baseDefinition);

    public string DisplayName
    {
        get
        {
            string prefix = prefixAffix != null ? prefixAffix.AffixName + " " : string.Empty;
            string suffix = suffixAffix != null ? " " + suffixAffix.AffixName : string.Empty;
            return $"{prefix}{BaseName}{suffix}".Trim();
        }
    }

    public EquipmentInstance(
        EquipmentRarity rarity,
        int itemTier,
        EquipmentBaseDefinition baseDefinition,
        EquipmentAffixDefinition prefixAffix,
        EquipmentAffixDefinition suffixAffix,
        List<EquipmentModifierRoll> implicitRolls,
        List<EquipmentModifierRoll> prefixRolls,
        List<EquipmentModifierRoll> suffixRolls)
    {
        instanceId = Guid.NewGuid().ToString("N");
        this.rarity = rarity;
        this.itemTier = itemTier;
        this.baseDefinition = baseDefinition;
        this.prefixAffix = prefixAffix;
        this.suffixAffix = suffixAffix;
        this.implicitRolls = implicitRolls ?? new List<EquipmentModifierRoll>();
        this.prefixRolls = prefixRolls ?? new List<EquipmentModifierRoll>();
        this.suffixRolls = suffixRolls ?? new List<EquipmentModifierRoll>();
    }

    public EquipmentInstance(
        string instanceId,
        EquipmentRarity rarity,
        int itemTier,
        EquipmentBaseDefinition baseDefinition,
        EquipmentAffixDefinition prefixAffix,
        EquipmentAffixDefinition suffixAffix,
        List<EquipmentModifierRoll> implicitRolls,
        List<EquipmentModifierRoll> prefixRolls,
        List<EquipmentModifierRoll> suffixRolls)
    {
        this.instanceId = string.IsNullOrWhiteSpace(instanceId) ? Guid.NewGuid().ToString("N") : instanceId;
        this.rarity = rarity;
        this.itemTier = itemTier;
        this.baseDefinition = baseDefinition;
        this.prefixAffix = prefixAffix;
        this.suffixAffix = suffixAffix;
        this.implicitRolls = implicitRolls ?? new List<EquipmentModifierRoll>();
        this.prefixRolls = prefixRolls ?? new List<EquipmentModifierRoll>();
        this.suffixRolls = suffixRolls ?? new List<EquipmentModifierRoll>();
    }
}
