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

    public static string Build(GeneratedMap map)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"<b>{map.DisplayName}</b>");
        builder.AppendLine();
        builder.Append(BuildStats(map));
        return builder.ToString().TrimEnd();
    }

    public static string BuildStats(GeneratedMap map)
    {
        StringBuilder builder = new StringBuilder();

        foreach (MapModifierValue modifier in map.modifiers)
        {
            builder.Append("+");
            builder.Append(modifier.percent.ToString("0.#"));
            builder.Append("% ");
            builder.AppendLine(Labels[modifier.statType]);
        }

        return builder.ToString().TrimEnd();
    }
}
