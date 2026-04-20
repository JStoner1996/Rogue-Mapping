using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    private const string MapCatalogResourcePath = "Maps/MapCatalog";
    public const string DefaultMapId = "default_map";

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

    private static MapCatalog loadedMapCatalog;
    private static readonly IReadOnlyList<MapBaseDefinition> EmptyBaseMaps = System.Array.Empty<MapBaseDefinition>();
    private static readonly List<MapModifierValue> EmptyModifiers = new();

    public static List<MapInstance> GenerateChoices(int count)
    {
        List<MapInstance> results = new List<MapInstance>(Mathf.Max(0, count));

        if (count <= 0)
        {
            return results;
        }

        results.Add(CreateDefaultMap());
        List<MapBaseDefinition> choicePool = GetNonDefaultBaseMaps();

        for (int i = 1; i < count; i++)
        {
            RefillChoicePool(choicePool);
            MapBaseDefinition baseMap = TakeRandom(choicePool);

            if (baseMap == null)
            {
                break;
            }

            results.Add(CreateGeneratedMap(baseMap));
        }

        return results;
    }

    public static MapInstance CreateDefaultMap(
        VictoryConditionType victoryConditionType = VictoryConditionType.Kills,
        int victoryTarget = 10)
    {
        return new MapInstance
        {
            baseMap = FindBaseMap(DefaultMapId),
            VictoryConditionType = victoryConditionType,
            VictoryTarget = Mathf.Max(1, victoryTarget),
        };
    }

    public static MapInstance CreateDroppedMap(int currentTier, MapDropSettings dropSettings)
    {
        MapDropSettings settings = dropSettings ?? new MapDropSettings();
        int targetTier = RollDroppedMapTier(currentTier, settings);
        MapBaseDefinition baseMap = RollBaseMapForTier(targetTier);

        if (baseMap == null)
        {
            Debug.LogWarning($"Unable to create dropped map for tier {targetTier}. Falling back to default map.");
            return CreateDefaultMap();
        }

        return CreateGeneratedMap(baseMap);
    }

    public static OwnedMapRecord CreateOwnedMapRecord(MapInstance map)
    {
        if (map == null || map.baseMap == null)
        {
            return null;
        }

        return new OwnedMapRecord
        {
            instanceId = System.Guid.NewGuid().ToString("N"),
            baseMapId = map.BaseMapId,
            prefixName = map.prefix != null ? map.prefix.name : string.Empty,
            suffixName = map.suffix != null ? map.suffix.name : string.Empty,
            victoryConditionType = map.VictoryConditionType,
            victoryTarget = Mathf.Max(1, map.VictoryTarget),
            modifiers = CopyModifiers(map.modifiers),
        };
    }

    public static MapInstance CreateMapInstanceFromRecord(OwnedMapRecord record)
    {
        if (record == null)
        {
            return null;
        }

        MapBaseDefinition baseMap = FindBaseMap(record.baseMapId);

        if (baseMap == null)
        {
            Debug.LogWarning($"Unable to rebuild owned map. Unknown base map id: {record.baseMapId}");
            return null;
        }

        return new MapInstance
        {
            baseMap = baseMap,
            prefix = MapAffixLibrary.FindPrefix(record.prefixName),
            suffix = MapAffixLibrary.FindSuffix(record.suffixName),
            VictoryConditionType = record.victoryConditionType,
            VictoryTarget = Mathf.Max(1, record.victoryTarget),
            modifiers = CopyModifiers(record.modifiers),
        };
    }

    public static MapBaseDefinition FindBaseMap(string baseMapId)
    {
        if (string.IsNullOrWhiteSpace(baseMapId))
        {
            return null;
        }

        List<MapBaseDefinition> baseMaps = GetBaseMapsInternal();

        for (int i = 0; i < baseMaps.Count; i++)
        {
            if (baseMaps[i] != null && baseMaps[i].id == baseMapId)
            {
                return baseMaps[i];
            }
        }

        return null;
    }

    public static IReadOnlyList<MapBaseDefinition> GetBaseMaps() => GetBaseMapsInternal();

    public static List<MapBaseDefinition> GetBaseMapsForTier(int tier)
    {
        return GetMatchingBaseMaps(map => IsNonDefaultBaseMap(map) && map.tier == tier);
    }

    private static void AssignVictoryCondition(MapInstance map)
    {
        if (map == null)
        {
            return;
        }

        map.VictoryConditionType = RollVictoryConditionType();
        map.VictoryTarget = map.VictoryConditionType == VictoryConditionType.Time
            ? GetTargetByRarity(TimeMinutesByRarity, map.Tier, map.Rarity, TimeMinutesPerTier)
            : GetTargetByRarity(KillsByRarity, map.Tier, map.Rarity, KillsPerTier);
    }

    private static VictoryConditionType RollVictoryConditionType() => Random.value < 0.5f ? VictoryConditionType.Time : VictoryConditionType.Kills;

    private static int GetTargetByRarity(int[] baseTargets, int tier, MapAffixTier rarity, int perTierValue)
    {
        int rarityIndex = Mathf.Clamp((int)rarity, 0, baseTargets.Length - 1);
        return baseTargets[rarityIndex] + (tier * perTierValue);
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
        if (affix == null || output == null) return;
        foreach (MapModifierRange modifier in affix.modifiers) output.Add(new MapModifierValue(modifier.statType, modifier.Roll()));
    }

    private static MapInstance CreateGeneratedMap(MapBaseDefinition baseMap)
    {
        MapInstance map = new MapInstance
        {
            baseMap = baseMap,
            prefix = MapAffixLibrary.RollPrefix(RollTier()),
            suffix = MapAffixLibrary.RollSuffix(RollTier()),
        };

        RollModifiersInto(map.prefix, map.modifiers);
        RollModifiersInto(map.suffix, map.modifiers);
        AssignVictoryCondition(map);
        return map;
    }

    private static int RollDroppedMapTier(int currentTier, MapDropSettings settings)
    {
        if (currentTier <= 0)
        {
            return GetNearestAvailableTierAtOrAbove(1);
        }

        List<TierWeightOption> candidates = new List<TierWeightOption>();
        AddTierWeightCandidate(candidates, currentTier, settings.sameTierWeight);
        AddTierWeightCandidate(candidates, currentTier + 1, settings.aboveTierWeight);
        if (currentTier > 1) AddTierWeightCandidate(candidates, currentTier - 1, settings.belowTierWeight);

        return RollTierFromCandidates(candidates, currentTier);
    }

    private static void AddTierWeightCandidate(List<TierWeightOption> candidates, int tier, float weight)
    {
        if (candidates == null || weight <= 0f || tier < 0 || !HasBaseMapsForTier(tier))
        {
            return;
        }

        candidates.Add(new TierWeightOption(tier, weight));
    }

    private static int RollTierFromCandidates(List<TierWeightOption> candidates, int fallbackTier = 0)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return GetNearestAvailableTierAtOrAbove(fallbackTier);
        }

        float totalWeight = 0f;

        foreach (TierWeightOption candidate in candidates)
        {
            totalWeight += candidate.weight;
        }

        float roll = Random.Range(0f, totalWeight);
        float runningWeight = 0f;

        foreach (TierWeightOption candidate in candidates)
        {
            runningWeight += candidate.weight;

            if (roll <= runningWeight)
            {
                return candidate.tier;
            }
        }

        return candidates[candidates.Count - 1].tier;
    }

    // Dropped maps ask for relative tiers, so this walks upward until it finds a tier the authored catalog can actually serve.
    private static int GetNearestAvailableTierAtOrAbove(int minimumTier)
    {
        int targetTier = Mathf.Max(0, minimumTier);

        while (targetTier <= 100)
        {
            if (HasBaseMapsForTier(targetTier))
            {
                return targetTier;
            }

            targetTier++;
        }

        return Mathf.Max(0, minimumTier);
    }

    private static void RefillChoicePool(List<MapBaseDefinition> choicePool)
    {
        if (choicePool == null || choicePool.Count > 0)
        {
            return;
        }

        // Choice generation wraps only after the player has been offered every authored non-default map once.
        choicePool.AddRange(GetNonDefaultBaseMaps());
    }

    private static MapBaseDefinition RollBaseMapForTier(int tier) => TakeRandom(GetBaseMapsForTier(tier));

    private static List<MapBaseDefinition> GetNonDefaultBaseMaps() => GetMatchingBaseMaps(IsNonDefaultBaseMap);

    private static List<MapBaseDefinition> GetMatchingBaseMaps(System.Predicate<MapBaseDefinition> match)
    {
        List<MapBaseDefinition> baseMaps = GetBaseMapsInternal();

        if (match == null)
        {
            return baseMaps;
        }

        return baseMaps.FindAll(match);
    }

    private static bool HasBaseMapsForTier(int tier)
    {
        List<MapBaseDefinition> baseMaps = GetBaseMapsInternal();

        for (int i = 0; i < baseMaps.Count; i++)
        {
            if (IsNonDefaultBaseMap(baseMaps[i]) && baseMaps[i].tier == tier)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNonDefaultBaseMap(MapBaseDefinition baseMap) => baseMap != null && baseMap.id != DefaultMapId;

    private static List<MapBaseDefinition> GetBaseMapsInternal()
    {
        EnsureCatalogLoaded();

        if (loadedMapCatalog != null && loadedMapCatalog.BaseMaps != null && loadedMapCatalog.BaseMaps.Count > 0)
        {
            return new List<MapBaseDefinition>(loadedMapCatalog.BaseMaps);
        }

        Debug.LogError($"MapCatalog asset not found or empty at Resources/{MapCatalogResourcePath}. Map generation requires a valid catalog.");
        return new List<MapBaseDefinition>(EmptyBaseMaps);
    }

    private static void EnsureCatalogLoaded()
    {
        if (loadedMapCatalog != null)
        {
            return;
        }

        loadedMapCatalog = Resources.Load<MapCatalog>(MapCatalogResourcePath);
    }

    private static T TakeRandom<T>(List<T> source)
    {
        if (source == null || source.Count == 0)
        {
            return default;
        }

        int index = Random.Range(0, source.Count);
        T value = source[index];
        source.RemoveAt(index);
        return value;
    }

    private static List<MapModifierValue> CopyModifiers(IReadOnlyList<MapModifierValue> modifiers) =>
        new(modifiers ?? EmptyModifiers);

    private struct TierWeightOption
    {
        public int tier;
        public float weight;

        public TierWeightOption(int tier, float weight)
        {
            this.tier = tier;
            this.weight = weight;
        }
    }
}
