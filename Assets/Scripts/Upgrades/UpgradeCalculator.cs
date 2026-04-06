using System.Collections.Generic;
using UnityEngine;

public static class UpgradeCalculator
{
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

        while (availableRolls.Count > 0 && currentWeight < maxWeight)
        {
            StatRoll roll = availableRolls[Random.Range(0, availableRolls.Count)];
            Debug.Log($"Attempting to add roll: {roll.statType} with weight {roll.weight:F2} (Current Weight: {currentWeight}, Max Weight: {maxWeight})");
            // Prevent exceeding rarity weight cap
            if (currentWeight + roll.weight > maxWeight)
            {
                availableRolls.Remove(roll);
                continue;
            }

            float value = Random.Range(roll.minValue, roll.maxValue);

            Debug.Log($"Rarity: {rarity} | Roll: {roll.statType} + {value:F2}");

            if (roll.statType == StatType.AttackSpeed)
            {
                value = Mathf.Abs(value);
            }

            result.AddStat(roll.statType, value);

            currentWeight += roll.weight;
            availableRolls.Remove(roll);

            result.weight = currentWeight;
        }

        return result;
    }
}