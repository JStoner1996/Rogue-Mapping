using System.Collections.Generic;
using UnityEngine;

public static class UpgradeCalculator
{
    public static WeaponUpgradeResult RollUpgrade(List<StatRoll> rolls, UpgradeRarity rarity)
    {
        WeaponUpgradeResult result = new WeaponUpgradeResult();

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
                maxWeight = 3;
                break;

            case UpgradeRarity.Uncommon:
                minWeight = 5;
                maxWeight = 7;
                break;

            case UpgradeRarity.Rare:
                minWeight = 9;
                maxWeight = 12;
                break;
        }

        int currentWeight = 0;

        List<StatRoll> availableRolls = new List<StatRoll>(rolls);

        while (availableRolls.Count > 0 && currentWeight < maxWeight)
        {
            StatRoll roll = availableRolls[Random.Range(0, availableRolls.Count)];

            // Prevent exceeding rarity budget
            if (currentWeight + roll.weight > maxWeight)
            {
                availableRolls.Remove(roll);
                continue;
            }

            float value = Random.Range(roll.minValue, roll.maxValue);

            Debug.Log($"Rarity: {rarity} | Roll: {roll.statType} + {value}");

            result.AddStat(roll.statType, value);

            currentWeight += roll.weight;
            availableRolls.Remove(roll);
        }

        return result;
    }
}