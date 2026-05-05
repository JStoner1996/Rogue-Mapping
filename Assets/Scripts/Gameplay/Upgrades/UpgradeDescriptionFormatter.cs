using System.Text;
using UnityEngine;

public static class UpgradeDescriptionFormatter
{
    public static string Build(WeaponUpgradeResult upgrade)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var stat in upgrade.stats)
        {
            sb.AppendLine(FormatStat(stat.Key, stat.Value));
        }

        // DEBUG: Show weight in description for testing purposes
        sb.AppendLine();
        sb.AppendLine($"Weight: {upgrade.weight:F2}");

        return sb.ToString();
    }

    private static string FormatStat(StatType statType, float value)
    {
        string statName = StringUtils.SplitCamelCase(statType.ToString());

        switch (statType)
        {
            case StatType.BounceCount:
                return $"{statName} +{Mathf.RoundToInt(value)}";

            case StatType.Damage:
            case StatType.CriticalChance:
            case StatType.CriticalDamage:
            case StatType.Knockback:
            case StatType.AttackSpeed:
            case StatType.Range:
                float percent = value * 100f;
                return $"{statName} +{percent:F0}%";

            case StatType.Duration:
            case StatType.Cooldown:
                return $"{statName} {value:F2}";

            default:
                return $"{statName} +{value:F2}";
        }
    }
}
