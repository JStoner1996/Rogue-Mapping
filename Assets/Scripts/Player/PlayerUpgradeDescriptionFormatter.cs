using System.Text;

public static class PlayerUpgradeDescriptionFormatter
{
    public static string Build(PlayerStatUpgradeResult upgrade)
    {
        StringBuilder builder = new StringBuilder();

        foreach (var stat in upgrade.stats)
        {
            builder.AppendLine(FormatStat(stat.Key, stat.Value));
        }

        builder.AppendLine();
        builder.AppendLine($"Weight: {upgrade.weight:F2}");

        return builder.ToString();
    }

    private static string FormatStat(PlayerStatType statType, float value)
    {
        string statName = StringUtils.SplitCamelCase(statType.ToString());

        return statType switch
        {
            PlayerStatType.Armor => $"{statName} +{value:F0}",
            PlayerStatType.HealthRegen => $"{statName} +{value:F1}/s",
            _ => $"{statName} +{value * 100f:F0}%"
        };
    }
}
