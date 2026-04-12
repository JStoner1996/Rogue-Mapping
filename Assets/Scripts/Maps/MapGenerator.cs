using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    private static readonly int[] TimeMinutesByRarity =
    {
        2,
        4,
        6,
    };

    private static readonly int[] KillsByRarity =
    {
        50,
        75,
        100,
    };

    private const int TimeMinutesPerTier = 2;
    private const int KillsPerTier = 25;

    private static readonly List<MapBaseDefinition> BaseMaps = new List<MapBaseDefinition>
    {
        new MapBaseDefinition("default_map", "Default Map", 0, MapTilesetTheme.Default, "Game"),
        new MapBaseDefinition("shrine", "Shrine", 1, MapTilesetTheme.Shrine, "Game"),
        new MapBaseDefinition("waterways", "Waterways", 2, MapTilesetTheme.Waterways, "Game"),
        new MapBaseDefinition("fields", "Fields", 3, MapTilesetTheme.Fields, "Game"),
        new MapBaseDefinition("valley", "Valley", 4, MapTilesetTheme.Valley, "Game"),
        new MapBaseDefinition("catacombs", "Catacombs", 5, MapTilesetTheme.Catacombs, "Game"),
        new MapBaseDefinition("grove", "Grove", 6, MapTilesetTheme.Grove, "Game"),
        new MapBaseDefinition("crossroads", "Crossroads", 7, MapTilesetTheme.Crossroads, "Game"),
        new MapBaseDefinition("sanctum", "Sanctum", 8, MapTilesetTheme.Sanctum, "Game"),
        new MapBaseDefinition("marsh", "Marsh", 9, MapTilesetTheme.Marsh, "Game"),
        new MapBaseDefinition("ruins", "Ruins", 10, MapTilesetTheme.Ruins, "Game"),
        new MapBaseDefinition("hollow", "Hollow", 10, MapTilesetTheme.Hollow, "Game"),
        new MapBaseDefinition("terrace", "Terrace", 10, MapTilesetTheme.Terrace, "Game"),
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

    public static List<MapInstance> GenerateChoices(int count)
    {
        List<MapInstance> results = new List<MapInstance>(count);

        if (count <= 0)
        {
            return results;
        }

        results.Add(CreateDefaultMap());

        List<MapBaseDefinition> availableBaseMaps = new List<MapBaseDefinition>(BaseMaps);
        availableBaseMaps.RemoveAll(map => map.id == "default_map");

        for (int i = 1; i < count; i++)
        {
            if (availableBaseMaps.Count == 0)
            {
                availableBaseMaps = new List<MapBaseDefinition>(BaseMaps);
                availableBaseMaps.RemoveAll(map => map.id == "default_map");
            }

            int baseMapIndex = Random.Range(0, availableBaseMaps.Count);
            MapBaseDefinition baseMap = availableBaseMaps[baseMapIndex];
            availableBaseMaps.RemoveAt(baseMapIndex);

            MapAffixDefinition prefix = RollAffix(Prefixes);
            MapAffixDefinition suffix = RollAffix(Suffixes);

            MapInstance map = new MapInstance
            {
                baseMap = baseMap,
                prefix = prefix,
                suffix = suffix,
            };

            RollModifiersInto(prefix, map.modifiers);
            RollModifiersInto(suffix, map.modifiers);
            AssignVictoryCondition(map);
            results.Add(map);
        }

        return results;
    }

    public static MapInstance CreateDefaultMap(
        VictoryConditionType victoryConditionType = VictoryConditionType.Kills,
        int victoryTarget = 10)
    {
        return new MapInstance
        {
            baseMap = BaseMaps.Find(map => map.id == "default_map"),
            VictoryConditionType = victoryConditionType,
            VictoryTarget = Mathf.Max(1, victoryTarget),
        };
    }

    private static void AssignVictoryCondition(MapInstance map)
    {
        map.VictoryConditionType = RollVictoryConditionType();
        map.VictoryTarget = map.VictoryConditionType == VictoryConditionType.Time
            ? GetTimeMinutesTarget(map.Tier, map.Rarity)
            : GetKillTarget(map.Tier, map.Rarity);
    }

    private static VictoryConditionType RollVictoryConditionType()
    {
        return Random.value < 0.5f ? VictoryConditionType.Time : VictoryConditionType.Kills;
    }

    private static int GetTimeMinutesTarget(int tier, MapAffixTier rarity)
    {
        return TimeMinutesByRarity[(int)rarity] + (tier * TimeMinutesPerTier);
    }

    private static int GetKillTarget(int tier, MapAffixTier rarity)
    {
        return KillsByRarity[(int)rarity] + (tier * KillsPerTier);
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
