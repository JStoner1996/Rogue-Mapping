using System.Collections.Generic;
using UnityEngine;

public static class PlayerUpgradeCalculator
{
    private const int MaxStatsPerUpgrade = 1;

    public static PlayerStatUpgradeResult RollUpgrade(List<PlayerStatRoll> rolls, UpgradeRarity rarity)
    {
        PlayerStatUpgradeResult result = new PlayerStatUpgradeResult
        {
            rarity = rarity,
        };

        if (rolls == null || rolls.Count == 0)
        {
            Debug.LogWarning("No player stat rolls provided!");
            return result;
        }

        int minWeight = 0;
        int maxWeight = 0;

        switch (rarity)
        {
            case UpgradeRarity.Common:
                minWeight = 1;
                maxWeight = 2;
                break;

            case UpgradeRarity.Uncommon:
                minWeight = 3;
                maxWeight = 5;
                break;

            case UpgradeRarity.Rare:
                minWeight = 6;
                maxWeight = 9;
                break;
        }

        maxWeight = Random.Range(minWeight, maxWeight + 1);

        int currentWeight = 0;
        List<PlayerStatRoll> availableRolls = new List<PlayerStatRoll>(rolls);
        HashSet<PlayerStatType> usedStats = new HashSet<PlayerStatType>();

        while (availableRolls.Count > 0 && currentWeight < maxWeight && usedStats.Count < MaxStatsPerUpgrade)
        {
            PlayerStatRoll roll = availableRolls[Random.Range(0, availableRolls.Count)];

            if (usedStats.Contains(roll.statType))
            {
                availableRolls.Remove(roll);
                continue;
            }

            if (currentWeight + roll.weight > maxWeight)
            {
                availableRolls.Remove(roll);
                continue;
            }

            float value = Random.Range(roll.minValue, roll.maxValue);
            result.AddStat(roll.statType, value, roll.usesFlatValue);

            usedStats.Add(roll.statType);
            currentWeight += roll.weight;
            availableRolls.Remove(roll);
            result.weight = currentWeight;
        }

        return result;
    }
}
