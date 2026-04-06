using System.Collections.Generic;

public class WeaponUpgradeResult
{
    public Dictionary<StatType, float> stats = new Dictionary<StatType, float>();
    public UpgradeRarity rarity;
    public float weight;

    public void AddStat(StatType type, float value)
    {
        if (!stats.ContainsKey(type))
            stats[type] = 0;

        stats[type] += value;
    }
}