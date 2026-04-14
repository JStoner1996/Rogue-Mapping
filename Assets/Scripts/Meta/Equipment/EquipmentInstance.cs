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
    [SerializeField] private List<EquipmentModifierRoll> implicitRolls = new List<EquipmentModifierRoll>();
    [SerializeField] private List<EquipmentRolledAffix> prefixAffixes = new List<EquipmentRolledAffix>();
    [SerializeField] private List<EquipmentRolledAffix> suffixAffixes = new List<EquipmentRolledAffix>();

    public string InstanceId => instanceId;
    public EquipmentRarity Rarity => rarity;
    public int ItemTier => itemTier;
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
        EquipmentBaseDefinition baseDefinition,
        List<EquipmentModifierRoll> implicitRolls,
        List<EquipmentRolledAffix> prefixAffixes,
        List<EquipmentRolledAffix> suffixAffixes)
    {
        instanceId = Guid.NewGuid().ToString("N");
        this.rarity = rarity;
        this.itemTier = itemTier;
        this.baseDefinition = baseDefinition;
        this.implicitRolls = implicitRolls ?? new List<EquipmentModifierRoll>();
        this.prefixAffixes = prefixAffixes ?? new List<EquipmentRolledAffix>();
        this.suffixAffixes = suffixAffixes ?? new List<EquipmentRolledAffix>();
    }

    public EquipmentInstance(
        string instanceId,
        EquipmentRarity rarity,
        int itemTier,
        EquipmentBaseDefinition baseDefinition,
        List<EquipmentModifierRoll> implicitRolls,
        List<EquipmentRolledAffix> prefixAffixes,
        List<EquipmentRolledAffix> suffixAffixes)
    {
        this.instanceId = string.IsNullOrWhiteSpace(instanceId) ? Guid.NewGuid().ToString("N") : instanceId;
        this.rarity = rarity;
        this.itemTier = itemTier;
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

        if (affixes == null)
        {
            return rolls;
        }

        for (int i = 0; i < affixes.Count; i++)
        {
            EquipmentRolledAffix affix = affixes[i];
            if (affix?.ModifierRolls == null)
            {
                continue;
            }

            for (int j = 0; j < affix.ModifierRolls.Count; j++)
            {
                rolls.Add(affix.ModifierRolls[j]);
            }
        }

        return rolls;
    }
}
