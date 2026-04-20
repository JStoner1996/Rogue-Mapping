using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EquipmentInstance
{
    private static readonly IReadOnlyList<EquipmentModifierRoll> EmptyRolls = Array.Empty<EquipmentModifierRoll>();

    [SerializeField] private string instanceId;
    [SerializeField] private EquipmentRarity rarity;
    [SerializeField] private int itemTier;
    [SerializeField] private int itemLevel;
    [SerializeField] private EquipmentBaseDefinition baseDefinition;
    [SerializeField] private List<EquipmentModifierRoll> implicitRolls = new List<EquipmentModifierRoll>();
    [SerializeField] private List<EquipmentRolledAffix> prefixAffixes = new List<EquipmentRolledAffix>();
    [SerializeField] private List<EquipmentRolledAffix> suffixAffixes = new List<EquipmentRolledAffix>();

    public string InstanceId => instanceId;
    public EquipmentRarity Rarity => rarity;
    public int ItemTier => itemTier;
    public int ItemLevel => itemLevel;
    public EquipmentBaseDefinition BaseDefinition => baseDefinition;
    public IReadOnlyList<EquipmentModifierRoll> ImplicitRolls => implicitRolls;
    public IReadOnlyList<EquipmentRolledAffix> PrefixAffixes => prefixAffixes;
    public IReadOnlyList<EquipmentRolledAffix> SuffixAffixes => suffixAffixes;
    public EquipmentAffixDefinition PrefixAffix => GetDisplayAffix(prefixAffixes);
    public EquipmentAffixDefinition SuffixAffix => GetDisplayAffix(suffixAffixes);
    public IReadOnlyList<EquipmentModifierRoll> PrefixRolls => CollectRolls(prefixAffixes);
    public IReadOnlyList<EquipmentModifierRoll> SuffixRolls => CollectRolls(suffixAffixes);

    public EquipmentSlotType SlotType => baseDefinition != null ? baseDefinition.SlotType : default;
    public string BaseName => baseDefinition != null ? baseDefinition.BaseName : string.Empty;
    public Sprite Icon => EquipmentIconResolver.ResolveIcon(baseDefinition);

    public string DisplayName
    {
        get
        {
            string prefix = PrefixAffix != null ? PrefixAffix.AffixName + " " : string.Empty;
            string suffix = SuffixAffix != null ? " " + SuffixAffix.AffixName : string.Empty;
            return $"{prefix}{BaseName}{suffix}".Trim();
        }
    }

    public EquipmentInstance(
        EquipmentRarity rarity,
        int itemTier,
        int itemLevel,
        EquipmentBaseDefinition baseDefinition,
        List<EquipmentModifierRoll> implicitRolls,
        List<EquipmentRolledAffix> prefixAffixes,
        List<EquipmentRolledAffix> suffixAffixes)
        : this(Guid.NewGuid().ToString("N"), rarity, itemTier, itemLevel, baseDefinition, implicitRolls, prefixAffixes, suffixAffixes)
    {
    }

    public EquipmentInstance(
        string instanceId,
        EquipmentRarity rarity,
        int itemTier,
        int itemLevel,
        EquipmentBaseDefinition baseDefinition,
        List<EquipmentModifierRoll> implicitRolls,
        List<EquipmentRolledAffix> prefixAffixes,
        List<EquipmentRolledAffix> suffixAffixes)
    {
        this.instanceId = string.IsNullOrWhiteSpace(instanceId) ? Guid.NewGuid().ToString("N") : instanceId;
        this.rarity = rarity;
        this.itemTier = itemTier;
        this.itemLevel = Mathf.Max(1, itemLevel);
        this.baseDefinition = baseDefinition;
        this.implicitRolls = implicitRolls ?? new List<EquipmentModifierRoll>();
        this.prefixAffixes = prefixAffixes ?? new List<EquipmentRolledAffix>();
        this.suffixAffixes = suffixAffixes ?? new List<EquipmentRolledAffix>();
    }

    private static EquipmentAffixDefinition GetDisplayAffix(IReadOnlyList<EquipmentRolledAffix> affixes)
    {
        EquipmentRolledAffix highestTierAffix = null;

        if (affixes == null)
        {
            return null;
        }

        for (int i = 0; i < affixes.Count; i++)
        {
            EquipmentRolledAffix affix = affixes[i];
            if (affix == null || affix.AffixDefinition == null)
            {
                continue;
            }

            if (highestTierAffix == null || affix.AffixTier > highestTierAffix.AffixTier)
            {
                highestTierAffix = affix;
            }
        }

        return highestTierAffix != null ? highestTierAffix.AffixDefinition : null;
    }

    private static IReadOnlyList<EquipmentModifierRoll> CollectRolls(IReadOnlyList<EquipmentRolledAffix> affixes)
    {
        List<EquipmentModifierRoll> rolls = new List<EquipmentModifierRoll>();
        if (affixes == null) return rolls;

        for (int i = 0; i < affixes.Count; i++)
        {
            IReadOnlyList<EquipmentModifierRoll> modifierRolls = affixes[i]?.ModifierRolls ?? EmptyRolls;
            for (int j = 0; j < modifierRolls.Count; j++) rolls.Add(modifierRolls[j]);
        }

        return rolls;
    }
}
