using System.Collections.Generic;

public class PlayerStatUpgradeResult
{
    public Dictionary<PlayerStatType, float> stats = new Dictionary<PlayerStatType, float>();
    public UpgradeRarity rarity;
    public float weight;

    public void AddStat(PlayerStatType type, float value)
    {
        if (!stats.ContainsKey(type))
        {
            stats[type] = 0f;
        }

        stats[type] += value;
    }
}
