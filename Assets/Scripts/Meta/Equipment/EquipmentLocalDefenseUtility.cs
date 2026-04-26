using System.Collections.Generic;
using UnityEngine;

public static class EquipmentLocalDefenseUtility
{
    private const string ModifiedImplicitColor = "#65E572";

    public static bool IsLocalDefenseStat(EquipmentStatType statType) =>
        statType == EquipmentStatType.Armor
        || statType == EquipmentStatType.Evasion
        || statType == EquipmentStatType.Barrier;

    public static bool BaseHasMatchingImplicit(EquipmentBaseDefinition baseDefinition, EquipmentStatType statType)
    {
        if (baseDefinition == null || !IsLocalDefenseStat(statType))
        {
            return false;
        }

        return ContainsStat(baseDefinition.ImplicitModifiers, statType);
    }

    public static bool AffixMatchesBaseImplicit(EquipmentAffixDefinition affix, EquipmentBaseDefinition baseDefinition)
    {
        if (affix == null)
        {
            return false;
        }

        foreach (EquipmentModifierDefinition modifier in affix.Modifiers)
        {
            if (IsLocalDefenseStat(modifier.statType) && !BaseHasMatchingImplicit(baseDefinition, modifier.statType))
            {
                return false;
            }
        }

        return true;
    }

    public static LocalDefenseTotals Calculate(EquipmentInstance item, EquipmentStatType statType)
    {
        if (item == null || !IsLocalDefenseStat(statType))
        {
            return new LocalDefenseTotals(statType);
        }

        float implicitValue = SumRolls(item.ImplicitRolls, statType, EquipmentModifierKind.Flat);
        if (Mathf.Approximately(implicitValue, 0f))
        {
            return new LocalDefenseTotals(statType);
        }

        float flatAffixValue = SumAffixRolls(item.PrefixAffixes, statType, EquipmentModifierKind.Flat)
            + SumAffixRolls(item.SuffixAffixes, statType, EquipmentModifierKind.Flat);
        float percentAffixValue = SumAffixRolls(item.PrefixAffixes, statType, EquipmentModifierKind.Percent)
            + SumAffixRolls(item.SuffixAffixes, statType, EquipmentModifierKind.Percent);

        return new LocalDefenseTotals(statType, implicitValue, flatAffixValue, percentAffixValue);
    }

    public static bool IsModifiedImplicit(LocalDefenseTotals totals) =>
        totals.HasImplicit
        && (!Mathf.Approximately(totals.flatAffixValue, 0f) || !Mathf.Approximately(totals.percentAffixValue, 0f));

    public static string WrapModifiedImplicit(string line) =>
        $"<color={ModifiedImplicitColor}>{line}</color>";

    private static bool ContainsStat(IReadOnlyList<EquipmentModifierDefinition> modifiers, EquipmentStatType statType)
    {
        if (modifiers == null)
        {
            return false;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].statType == statType)
            {
                return true;
            }
        }

        return false;
    }

    private static float SumAffixRolls(
        IReadOnlyList<EquipmentRolledAffix> affixes,
        EquipmentStatType statType,
        EquipmentModifierKind modifierKind)
    {
        if (affixes == null)
        {
            return 0f;
        }

        float total = 0f;
        for (int i = 0; i < affixes.Count; i++)
        {
            total += SumRolls(affixes[i]?.ModifierRolls, statType, modifierKind);
        }

        return total;
    }

    private static float SumRolls(
        IReadOnlyList<EquipmentModifierRoll> rolls,
        EquipmentStatType statType,
        EquipmentModifierKind modifierKind)
    {
        if (rolls == null)
        {
            return 0f;
        }

        float total = 0f;
        for (int i = 0; i < rolls.Count; i++)
        {
            EquipmentModifierRoll roll = rolls[i];
            if (roll.statType == statType && roll.modifierKind == modifierKind)
            {
                total += roll.value;
            }
        }

        return total;
    }

    public readonly struct LocalDefenseTotals
    {
        public readonly EquipmentStatType statType;
        public readonly float implicitValue;
        public readonly float flatAffixValue;
        public readonly float percentAffixValue;

        public LocalDefenseTotals(
            EquipmentStatType statType,
            float implicitValue = 0f,
            float flatAffixValue = 0f,
            float percentAffixValue = 0f)
        {
            this.statType = statType;
            this.implicitValue = implicitValue;
            this.flatAffixValue = flatAffixValue;
            this.percentAffixValue = percentAffixValue;
        }

        public bool HasImplicit => !Mathf.Approximately(implicitValue, 0f);
        public float FinalValue => (implicitValue + flatAffixValue) * (1f + percentAffixValue);
    }
}
