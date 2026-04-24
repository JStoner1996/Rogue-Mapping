using System.Collections.Generic;
using System.Text;

public static class MapDescriptionFormatter
{
    private static readonly Dictionary<MapStatType, string> Labels = new Dictionary<MapStatType, string>
    {
        { MapStatType.EnemyQuantity, "Enemy Quantity" },
        { MapStatType.EnemyQuality, "Enemy Quality" },
        { MapStatType.DropChance, "Drop Chance" },
        { MapStatType.MapDropChance, "Map Drop Chance" },
        { MapStatType.MapRarity, "Map Quality" },
        { MapStatType.EquipmentDropChance, "Equipment Drop Chance" },
        { MapStatType.EquipmentRarity, "Equipment Quality" },
        { MapStatType.EnemyDamage, "Enemy Damage" },
        { MapStatType.EnemyHealth, "Enemy Health" },
        { MapStatType.EnemyMoveSpeed, "Enemy Movement Speed" },
        { MapStatType.ExperienceWorth, "Experience Gain" },
        { MapStatType.EliteChance, "Elite Chance" },
        { MapStatType.TankChance, "Tank Chance" },
        { MapStatType.SkirmisherChance, "Skirmisher Chance" },
        { MapStatType.EliteDamage, "Elite Damage" },
        { MapStatType.EliteHealth, "Elite Health" },
        { MapStatType.TankDamage, "Tank Damage" },
        { MapStatType.TankHealth, "Tank Health" },
        { MapStatType.SkirmisherDamage, "Skirmisher Damage" },
        { MapStatType.SkirmisherHealth, "Skirmisher Health" },
    };

    public static string Build(MapInstance map)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"<b>{map.DisplayName}</b>");
        builder.AppendLine();
        builder.Append(BuildStats(map));
        return builder.ToString().TrimEnd();
    }

    public static string BuildStats(MapInstance map)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine($"Tier: {map.Tier}");
        builder.AppendLine($"Rarity: {map.Rarity}");
        builder.AppendLine($"Completed: {(map.IsBaseMapCompleted ? "Yes" : "No")}");
        builder.AppendLine($"Victory: {FormatVictoryCondition(map)}");
        builder.AppendLine("__________________________");
        AppendAffixSummary(builder, map);

        return builder.ToString().TrimEnd();
    }

    private static void AppendAffixSummary(StringBuilder builder, MapInstance map)
    {
        if (map == null)
        {
            return;
        }

        IReadOnlyList<MapRolledAffix> affixes = map.GetAllAffixes();
        if (affixes == null || affixes.Count == 0)
        {
            return;
        }

        AppendAffixRolls(builder, map.PrefixAffixes);
        AppendAffixRolls(builder, map.SuffixAffixes);
        AppendAffixRolls(builder, map.AdditionalAffixes);
    }

    private static void AppendAffixRolls(StringBuilder builder, IReadOnlyList<MapRolledAffix> affixes)
    {
        if (builder == null || affixes == null)
        {
            return;
        }

        for (int i = 0; i < affixes.Count; i++)
        {
            MapRolledAffix affix = affixes[i];
            if (affix?.ModifierRolls == null)
            {
                continue;
            }

            for (int j = 0; j < affix.ModifierRolls.Count; j++)
            {
                MapModifierValue modifier = affix.ModifierRolls[j];
                builder.Append('+');
                builder.Append(modifier.percent.ToString("0.#"));
                builder.Append("% ");
                builder.AppendLine(Labels[modifier.statType]);
            }
        }
    }

    public static string FormatVictoryCondition(MapInstance map)
    {
        switch (map.VictoryConditionType)
        {
            case VictoryConditionType.Time:
                return $"Survive {map.VictoryTarget} minute{(map.VictoryTarget == 1 ? string.Empty : "s")}";

            case VictoryConditionType.Kills:
                return $"Defeat {map.VictoryTarget} enemies";

            default:
                return "Unknown";
        }
    }
}
