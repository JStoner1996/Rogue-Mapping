using System.Collections.Generic;
using UnityEngine;

// Keeps the temporary hard-coded affix definitions out of MapGenerator until they become authored assets.
public static class MapAffixLibrary
{
    private static readonly IReadOnlyList<MapAffixDefinition> Prefixes = new List<MapAffixDefinition>
    {
        new MapAffixDefinition("Gathering", MapAffixTier.Common,
            new MapModifierRange(MapStatType.EnemyQuantity, 8f, 14f),
            new MapModifierRange(MapStatType.EnemyQuality, 4f, 8f)),
        new MapAffixDefinition("Refined", MapAffixTier.Common,
            new MapModifierRange(MapStatType.EnemyQuantity, 6f, 10f),
            new MapModifierRange(MapStatType.EnemyQuality, 8f, 14f)),
        new MapAffixDefinition("Bountiful", MapAffixTier.Common,
            new MapModifierRange(MapStatType.EnemyQuantity, 10f, 16f),
            new MapModifierRange(MapStatType.EnemyQuality, 6f, 10f)),
        new MapAffixDefinition("Conquerer's", MapAffixTier.Uncommon,
            new MapModifierRange(MapStatType.EnemyQuantity, 14f, 20f),
            new MapModifierRange(MapStatType.EnemyQuality, 10f, 16f),
            new MapModifierRange(MapStatType.DropChance, 12f, 18f)),
        new MapAffixDefinition("Abhorrent", MapAffixTier.Uncommon,
            new MapModifierRange(MapStatType.EnemyQuantity, 12f, 18f),
            new MapModifierRange(MapStatType.EnemyQuality, 14f, 20f),
            new MapModifierRange(MapStatType.DropChance, 14f, 20f)),
        new MapAffixDefinition("Plunderer's", MapAffixTier.Uncommon,
            new MapModifierRange(MapStatType.EnemyQuantity, 13f, 19f),
            new MapModifierRange(MapStatType.EnemyQuality, 11f, 17f),
            new MapModifierRange(MapStatType.DropChance, 18f, 26f)),
        new MapAffixDefinition("Mythic", MapAffixTier.Rare,
            new MapModifierRange(MapStatType.EnemyQuantity, 20f, 30f),
            new MapModifierRange(MapStatType.EnemyQuality, 14f, 22f),
            new MapModifierRange(MapStatType.DropChance, 20f, 30f)),
        new MapAffixDefinition("Tyrannical", MapAffixTier.Rare,
            new MapModifierRange(MapStatType.EnemyQuantity, 18f, 26f),
            new MapModifierRange(MapStatType.EnemyQuality, 20f, 30f),
            new MapModifierRange(MapStatType.DropChance, 22f, 32f)),
        new MapAffixDefinition("God-Touched", MapAffixTier.Rare,
            new MapModifierRange(MapStatType.EnemyQuantity, 22f, 32f),
            new MapModifierRange(MapStatType.EnemyQuality, 18f, 26f),
            new MapModifierRange(MapStatType.DropChance, 30f, 45f)),
    };

    private static readonly IReadOnlyList<MapAffixDefinition> Suffixes = new List<MapAffixDefinition>
    {
        new MapAffixDefinition("of Embers", MapAffixTier.Common,
            new MapModifierRange(MapStatType.EnemyDamage, 10f, 16f),
            new MapModifierRange(MapStatType.EnemyHealth, 6f, 10f)),
        new MapAffixDefinition("of the Boar", MapAffixTier.Common,
            new MapModifierRange(MapStatType.EnemyDamage, 8f, 12f),
            new MapModifierRange(MapStatType.EnemyHealth, 14f, 22f)),
        new MapAffixDefinition("of Stone", MapAffixTier.Common,
            new MapModifierRange(MapStatType.EnemyDamage, 7f, 11f),
            new MapModifierRange(MapStatType.EnemyHealth, 18f, 26f)),
        new MapAffixDefinition("of Fury", MapAffixTier.Uncommon,
            new MapModifierRange(MapStatType.EnemyDamage, 16f, 24f),
            new MapModifierRange(MapStatType.EnemyHealth, 18f, 26f),
            new MapModifierRange(MapStatType.EnemyMoveSpeed, 6f, 10f)),
        new MapAffixDefinition("of Swiftness", MapAffixTier.Uncommon,
            new MapModifierRange(MapStatType.EnemyDamage, 12f, 18f),
            new MapModifierRange(MapStatType.EnemyHealth, 16f, 24f),
            new MapModifierRange(MapStatType.EnemyMoveSpeed, 12f, 18f)),
        new MapAffixDefinition("of the Hunt", MapAffixTier.Uncommon,
            new MapModifierRange(MapStatType.EnemyDamage, 14f, 20f),
            new MapModifierRange(MapStatType.EnemyHealth, 20f, 28f),
            new MapModifierRange(MapStatType.EnemyMoveSpeed, 10f, 14f)),
        new MapAffixDefinition("of Power", MapAffixTier.Rare,
            new MapModifierRange(MapStatType.EnemyDamage, 24f, 34f),
            new MapModifierRange(MapStatType.EnemyHealth, 22f, 32f),
            new MapModifierRange(MapStatType.EnemyMoveSpeed, 12f, 18f),
            new MapModifierRange(MapStatType.ExperienceWorth, 10f, 16f)),
        new MapAffixDefinition("of the Abyss", MapAffixTier.Rare,
            new MapModifierRange(MapStatType.EnemyDamage, 22f, 30f),
            new MapModifierRange(MapStatType.EnemyHealth, 28f, 40f),
            new MapModifierRange(MapStatType.EnemyMoveSpeed, 14f, 20f),
            new MapModifierRange(MapStatType.ExperienceWorth, 14f, 20f)),
        new MapAffixDefinition("of Endurance", MapAffixTier.Rare,
            new MapModifierRange(MapStatType.EnemyDamage, 18f, 26f),
            new MapModifierRange(MapStatType.EnemyHealth, 40f, 55f),
            new MapModifierRange(MapStatType.EnemyMoveSpeed, 10f, 16f),
            new MapModifierRange(MapStatType.ExperienceWorth, 12f, 18f)),
    };

    public static MapAffixDefinition RollPrefix(MapAffixTier tier) => RollAffix(Prefixes, tier);
    public static MapAffixDefinition RollSuffix(MapAffixTier tier) => RollAffix(Suffixes, tier);
    public static MapAffixDefinition FindPrefix(string affixName) => FindAffix(Prefixes, affixName);
    public static MapAffixDefinition FindSuffix(string affixName) => FindAffix(Suffixes, affixName);
    public static MapAffixDefinition FindAnyAffix(string affixName) => FindAffix(GetAllAffixes(), affixName);
    public static MapAffixDefinition RollAnyAffix(MapAffixTier tier, IReadOnlyCollection<string> excludedNames = null) =>
        RollAffix(GetAllAffixes(), tier, excludedNames);

    private static MapAffixDefinition RollAffix(
        IReadOnlyList<MapAffixDefinition> source,
        MapAffixTier tier,
        IReadOnlyCollection<string> excludedNames = null)
    {
        if (source == null)
        {
            return null;
        }

        List<MapAffixDefinition> candidates = new List<MapAffixDefinition>();

        for (int i = 0; i < source.Count; i++)
        {
            if (source[i] != null
                && source[i].tier == tier
                && (excludedNames == null || !IsExcludedAffix(excludedNames, source[i].name)))
            {
                candidates.Add(source[i]);
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    private static MapAffixDefinition FindAffix(IReadOnlyList<MapAffixDefinition> source, string affixName)
    {
        if (source == null || string.IsNullOrWhiteSpace(affixName))
        {
            return null;
        }

        for (int i = 0; i < source.Count; i++)
        {
            if (source[i] != null && source[i].name == affixName)
            {
                return source[i];
            }
        }

        return null;
    }

    private static IReadOnlyList<MapAffixDefinition> GetAllAffixes()
    {
        List<MapAffixDefinition> allAffixes = new List<MapAffixDefinition>(Prefixes.Count + Suffixes.Count);
        allAffixes.AddRange(Prefixes);
        allAffixes.AddRange(Suffixes);
        return allAffixes;
    }

    private static bool IsExcludedAffix(IReadOnlyCollection<string> excludedNames, string affixName)
    {
        if (excludedNames == null || string.IsNullOrWhiteSpace(affixName))
        {
            return false;
        }

        foreach (string excludedName in excludedNames)
        {
            if (excludedName == affixName)
            {
                return true;
            }
        }

        return false;
    }
}
