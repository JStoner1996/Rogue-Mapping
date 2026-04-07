using System.Collections.Generic;
using UnityEngine;


public static class UpgradeCalculator
{

    const int MAX_STATS_PER_UPGRADE = 3;

    public static UpgradeRarity RollRarity(
        float commonChance = 0.4f,
        float uncommonChance = 0.3f,
        float rareChance = 0.3f)
    {
        float rand = Random.value;

        if (rand < commonChance)
            return UpgradeRarity.Common;

        if (rand < commonChance + uncommonChance)
            return UpgradeRarity.Uncommon;

        return UpgradeRarity.Rare;
    }

    public static WeaponUpgradeResult RollUpgrade(List<StatRoll> rolls, UpgradeRarity rarity)
    {
        WeaponUpgradeResult result = new WeaponUpgradeResult();
        result.rarity = rarity;

        if (rolls == null || rolls.Count == 0)
        {
            Debug.LogWarning("No rolls provided!");
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

        List<StatRoll> availableRolls = new List<StatRoll>(rolls);
        HashSet<StatType> usedStats = new HashSet<StatType>();


        while (availableRolls.Count > 0 && currentWeight < maxWeight && usedStats.Count < MAX_STATS_PER_UPGRADE)
        {
            StatRoll roll = availableRolls[Random.Range(0, availableRolls.Count)];

            // Prevent duplicate stat types
            if (usedStats.Contains(roll.statType))
            {
                availableRolls.Remove(roll);
                continue;
            }

            // Prevent exceeding weight cap
            if (currentWeight + roll.weight > maxWeight)
            {
                availableRolls.Remove(roll);
                continue;
            }

            float value = Random.Range(roll.minValue, roll.maxValue);

            if (roll.statType == StatType.AttackSpeed)
            {
                value = Mathf.Abs(value);
            }

            result.AddStat(roll.statType, value);

            usedStats.Add(roll.statType);
            currentWeight += roll.weight;

            availableRolls.Remove(roll);

            result.weight = currentWeight;
        }

        return result;
    }
    public static List<StatRoll> FilterRolls(List<StatRoll> rolls, HashSet<StatType> allowed)
    {
        return rolls.FindAll(r => allowed.Contains(r.statType));
    }
}