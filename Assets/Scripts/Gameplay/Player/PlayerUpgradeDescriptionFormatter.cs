using System.Text;

public static class PlayerUpgradeDescriptionFormatter
{
    public static string Build(PlayerStatUpgradeResult upgrade)
    {
        StringBuilder builder = new StringBuilder();

        foreach (PlayerStatUpgradeResult.PlayerStatUpgradeEntry stat in upgrade.GetEntries())
        {
            builder.AppendLine(FormatStat(stat.statType, stat.value, stat.usesFlatValue));
        }

        builder.AppendLine();
        builder.AppendLine($"Weight: {upgrade.weight:F2}");

        return builder.ToString();
    }

    private static string FormatStat(PlayerStatType statType, float value, bool usesFlatValue)
    {
        string statName = StringUtils.SplitCamelCase(statType.ToString());

        if (usesFlatValue)
        {
            return statType switch
            {
                PlayerStatType.HealthRegen => $"{statName} +{value:F1}/s",
                _ => $"{statName} +{value:F0}",
            };
        }

        return $"{statName} +{value * 100f:F0}%";
    }
}
