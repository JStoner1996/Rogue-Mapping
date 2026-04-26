using System.Collections.Generic;

public static class EquipmentStatSummaryCalculator
{
    public static EquipmentStatSummary Calculate(IReadOnlyList<EquipmentInstance> equippedItems)
    {
        EquipmentStatSummary summary = new EquipmentStatSummary();

        if (equippedItems == null)
        {
            return summary;
        }

        for (int i = 0; i < equippedItems.Count; i++)
        {
            AddItem(summary, equippedItems[i]);
        }

        return summary;
    }

    private static void AddItem(EquipmentStatSummary summary, EquipmentInstance item)
    {
        if (summary == null || item == null)
        {
            return;
        }

        AddImplicitRolls(summary, item);
        AddAffixRolls(summary, item, item.PrefixAffixes);
        AddAffixRolls(summary, item, item.SuffixAffixes);
    }

    private static void AddImplicitRolls(EquipmentStatSummary summary, EquipmentInstance item)
    {
        if (item?.ImplicitRolls == null)
        {
            return;
        }

        for (int i = 0; i < item.ImplicitRolls.Count; i++)
        {
            EquipmentModifierRoll roll = item.ImplicitRolls[i];
            if (!EquipmentLocalDefenseUtility.IsLocalDefenseStat(roll.statType))
            {
                summary.AddRoll(roll);
                continue;
            }

            EquipmentLocalDefenseUtility.LocalDefenseTotals totals = EquipmentLocalDefenseUtility.Calculate(item, roll.statType);
            if (totals.HasImplicit)
            {
                summary.AddRoll(new EquipmentModifierRoll(roll.statType, EquipmentModifierKind.Flat, totals.FinalValue));
            }
        }
    }

    private static void AddAffixRolls(EquipmentStatSummary summary, EquipmentInstance item, IReadOnlyList<EquipmentRolledAffix> affixes)
    {
        if (affixes == null)
        {
            return;
        }

        for (int i = 0; i < affixes.Count; i++)
        {
            AddRolls(summary, item, affixes[i]?.ModifierRolls);
        }
    }

    private static void AddRolls(EquipmentStatSummary summary, EquipmentInstance item, IReadOnlyList<EquipmentModifierRoll> rolls)
    {
        if (rolls == null)
        {
            return;
        }

        for (int i = 0; i < rolls.Count; i++)
        {
            EquipmentModifierRoll roll = rolls[i];
            if (EquipmentLocalDefenseUtility.IsLocalDefenseStat(roll.statType)
                && EquipmentLocalDefenseUtility.Calculate(item, roll.statType).HasImplicit)
            {
                continue;
            }

            summary.AddRoll(roll);
        }
    }
}
