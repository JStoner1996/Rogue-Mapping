using System.Collections.Generic;
using System.Text;

public static class MapDescriptionFormatter
{
    private static readonly Dictionary<MapStatType, string> Labels = new Dictionary<MapStatType, string>
    {
        { MapStatType.EnemyQuantity, "Enemy Quantity" },
        { MapStatType.EnemyQuality, "Enemy Quality" },
        { MapStatType.DropChance, "Drop Chance" },
        { MapStatType.EnemyDamage, "Enemy Damage" },
        { MapStatType.EnemyHealth, "Enemy Health" },
        { MapStatType.EnemyMoveSpeed, "Enemy Movement Speed" },
        { MapStatType.ExperienceWorth, "Experience Worth" },
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
        builder.AppendLine($"Tileset: {StringUtils.SplitCamelCase(map.TilesetTheme.ToString())}");
        builder.AppendLine($"Completed: {(map.IsBaseMapCompleted ? "Yes" : "No")}");
        builder.AppendLine($"Victory: {FormatVictoryCondition(map)}");

        if (map.modifiers.Count > 0)
        {
            builder.AppendLine();
        }
        else
        {
            builder.AppendLine();
            builder.AppendLine("No modifiers");
        }

        foreach (MapModifierValue modifier in map.modifiers)
        {
            builder.Append("+");
            builder.Append(modifier.percent.ToString("0.#"));
            builder.Append("% ");
            builder.AppendLine(Labels[modifier.statType]);
        }

        return builder.ToString().TrimEnd();
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
