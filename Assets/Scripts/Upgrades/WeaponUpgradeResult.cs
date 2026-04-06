using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgradeResult
{
    public Dictionary<StatType, float> stats = new Dictionary<StatType, float>();
    public UpgradeRarity rarity;
    public float weight;

    public void AddStat(StatType type, float value)
    {
        if (!stats.ContainsKey(type))
            stats[type] = 0;

        // Special handling for int-based stats
        if (type == StatType.BounceCount)
        {
            stats[type] += Mathf.RoundToInt(value);
        }
        else
        {
            stats[type] += value;
        }
    }
}