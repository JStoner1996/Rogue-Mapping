using System.Collections.Generic;
using System.Text;

public static class EquipmentDescriptionFormatter
{
    private const string Divider = "--------------------";

    public static string BuildStats(EquipmentInstance item)
    {
        if (item == null)
        {
            return "No Equipment";
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Tier: {item.ItemTier}");
        builder.AppendLine($"Item Level: {item.ItemLevel}");
        builder.AppendLine($"Rarity: {item.Rarity}");
        builder.AppendLine($"Slot: {item.SlotType}");

        bool hasImplicit = item.ImplicitRolls != null && item.ImplicitRolls.Count > 0;
        bool hasExplicit = (item.PrefixRolls != null && item.PrefixRolls.Count > 0)
            || (item.SuffixRolls != null && item.SuffixRolls.Count > 0);

        if (hasImplicit)
        {
            builder.AppendLine();
            builder.AppendLine(Divider);
            AppendImplicitRolls(builder, item);
        }

        if (hasExplicit)
        {
            builder.AppendLine(Divider);
            AppendAffixRolls(builder, item.PrefixAffixes);
            AppendAffixRolls(builder, item.SuffixAffixes);
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendRolls(StringBuilder builder, IReadOnlyList<EquipmentModifierRoll> rolls)
    {
        if (rolls == null || rolls.Count == 0)
        {
            return;
        }

        for (int i = 0; i < rolls.Count; i++)
        {
            builder.AppendLine(FormatRoll(rolls[i]));
        }
    }

    private static void AppendImplicitRolls(StringBuilder builder, EquipmentInstance item)
    {
        if (item?.ImplicitRolls == null || item.ImplicitRolls.Count == 0)
        {
            return;
        }

        for (int i = 0; i < item.ImplicitRolls.Count; i++)
        {
            EquipmentModifierRoll roll = item.ImplicitRolls[i];
            if (!EquipmentLocalDefenseUtility.IsLocalDefenseStat(roll.statType))
            {
                builder.AppendLine(FormatRoll(roll));
                continue;
            }

            EquipmentLocalDefenseUtility.LocalDefenseTotals totals = EquipmentLocalDefenseUtility.Calculate(item, roll.statType);
            string line = FormatRoll(new EquipmentModifierRoll(roll.statType, EquipmentModifierKind.Flat, totals.FinalValue));
            builder.AppendLine(EquipmentLocalDefenseUtility.IsModifiedImplicit(totals)
                ? EquipmentLocalDefenseUtility.WrapModifiedImplicit(line)
                : line);
        }
    }

    private static void AppendAffixRolls(StringBuilder builder, IReadOnlyList<EquipmentRolledAffix> affixes)
    {
        if (affixes == null || affixes.Count == 0)
        {
            return;
        }

        for (int i = 0; i < affixes.Count; i++)
        {
            EquipmentRolledAffix affix = affixes[i];
            AppendRolls(builder, affix?.ModifierRolls);
        }
    }

    private static string FormatRoll(EquipmentModifierRoll roll)
    {
        string statName = StringUtils.SplitCamelCase(roll.statType.ToString());
        return roll.modifierKind == EquipmentModifierKind.Percent
            ? $"+{roll.value * 100f:F1}% {statName}"
            : $"+{roll.value:F1} {statName}";
    }
}
