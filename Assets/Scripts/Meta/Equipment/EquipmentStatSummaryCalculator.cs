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

        AddRolls(summary, item.ImplicitRolls);
        AddAffixRolls(summary, item.PrefixAffixes);
        AddAffixRolls(summary, item.SuffixAffixes);
    }

    private static void AddAffixRolls(EquipmentStatSummary summary, IReadOnlyList<EquipmentRolledAffix> affixes)
    {
        if (affixes == null)
        {
            return;
        }

        for (int i = 0; i < affixes.Count; i++)
        {
            AddRolls(summary, affixes[i]?.ModifierRolls);
        }
    }

    private static void AddRolls(EquipmentStatSummary summary, IReadOnlyList<EquipmentModifierRoll> rolls)
    {
        if (rolls == null)
        {
            return;
        }

        for (int i = 0; i < rolls.Count; i++)
        {
            summary.AddRoll(rolls[i]);
        }
    }
}
