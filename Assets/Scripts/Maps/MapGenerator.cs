using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    private static readonly string[] BaseMapNames =
    {
        "Shrine",
        "Waterways",
        "Fields",
        "Valley",
        "Catacombs",
        "Grove",
        "Crossroads",
        "Sanctum",
        "Marsh",
        "Ruins",
        "Hollow",
        "Terrace",
    };

    private static readonly List<MapAffixDefinition> Prefixes = new List<MapAffixDefinition>
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

    private static readonly List<MapAffixDefinition> Suffixes = new List<MapAffixDefinition>
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

    public static List<GeneratedMap> GenerateChoices(int count)
    {
        List<GeneratedMap> results = new List<GeneratedMap>(count);

        if (count <= 0)
        {
            return results;
        }

        results.Add(CreateDefaultMap());

        List<string> availableNames = new List<string>(BaseMapNames);
        availableNames.Remove("Shrine");

        for (int i = 1; i < count; i++)
        {
            if (availableNames.Count == 0)
            {
                availableNames = new List<string>(BaseMapNames);
                availableNames.Remove("Shrine");
            }

            int nameIndex = Random.Range(0, availableNames.Count);
            string baseName = availableNames[nameIndex];
            availableNames.RemoveAt(nameIndex);

            MapAffixDefinition prefix = RollAffix(Prefixes);
            MapAffixDefinition suffix = RollAffix(Suffixes);

            GeneratedMap map = new GeneratedMap
            {
                baseName = baseName,
                prefix = prefix,
                suffix = suffix,
            };

            RollModifiersInto(prefix, map.modifiers);
            RollModifiersInto(suffix, map.modifiers);
            results.Add(map);
        }

        return results;
    }

    private static GeneratedMap CreateDefaultMap()
    {
        return new GeneratedMap
        {
            baseName = "Default Map",
        };
    }

    private static MapAffixDefinition RollAffix(List<MapAffixDefinition> source)
    {
        MapAffixTier tier = RollTier();
        List<MapAffixDefinition> candidates = source.FindAll(entry => entry.tier == tier);
        return candidates[Random.Range(0, candidates.Count)];
    }

    private static MapAffixTier RollTier()
    {
        float roll = Random.value;

        if (roll < 0.18f)
        {
            return MapAffixTier.Rare;
        }

        if (roll < 0.52f)
        {
            return MapAffixTier.Uncommon;
        }

        return MapAffixTier.Common;
    }

    private static void RollModifiersInto(MapAffixDefinition affix, List<MapModifierValue> output)
    {
        foreach (MapModifierRange modifier in affix.modifiers)
        {
            output.Add(new MapModifierValue(modifier.statType, modifier.Roll()));
        }
    }
}
