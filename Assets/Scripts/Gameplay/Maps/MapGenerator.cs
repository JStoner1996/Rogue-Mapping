using System;
using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    public const string DefaultMapId = "default_map";
    private const float CommonMapRarityWeight = 48f;
    private const float UncommonMapRarityWeight = 34f;
    private const float RareMapRarityWeight = 18f;

    private static readonly int[] TimeMinutesByRarity = { 2, 4, 6 };
    private static readonly int[] KillsByRarity = { 50, 75, 100 };

    private const int TimeMinutesPerTier = 2;
    private const int KillsPerTier = 25;

    private readonly struct DroppedMapAtlasSettings
    {
        public readonly float rarityMultiplier;
        public readonly int additionalAffixCount;
        public readonly float higherTierBonus;
        public readonly bool lowerTierMapsNeverDrop;

        public DroppedMapAtlasSettings(
            float rarityMultiplier,
            int additionalAffixCount,
            float higherTierBonus,
            bool lowerTierMapsNeverDrop)
        {
            this.rarityMultiplier = rarityMultiplier;
            this.additionalAffixCount = additionalAffixCount;
            this.higherTierBonus = higherTierBonus;
            this.lowerTierMapsNeverDrop = lowerTierMapsNeverDrop;
        }
    }

    private readonly struct AffixRollState
    {
        public readonly HashSet<string> usedAffixNames;
        public readonly HashSet<MapStatType> usedStatTypes;

        public AffixRollState(HashSet<string> usedAffixNames, HashSet<MapStatType> usedStatTypes)
        {
            this.usedAffixNames = usedAffixNames;
            this.usedStatTypes = usedStatTypes;
        }
    }

    public static List<MapInstance> GenerateChoices(int count)
    {
        List<MapInstance> results = new List<MapInstance>(Mathf.Max(0, count));
        if (count <= 0)
        {
            return results;
        }

        MapInstance defaultMap = CreateDefaultMap();
        if (defaultMap != null)
        {
            results.Add(defaultMap);
        }

        List<MapBaseDefinition> choicePool = GetNonDefaultBaseMaps();

        for (int i = results.Count; i < count; i++)
        {
            RefillChoicePool(choicePool);
            MapBaseDefinition baseDefinition = TakeRandom(choicePool);
            if (baseDefinition == null)
            {
                break;
            }

            results.Add(CreateGeneratedMap(baseDefinition));
        }

        return results;
    }

    public static MapInstance CreateDefaultMap(
        VictoryConditionType victoryConditionType = VictoryConditionType.Kills,
        int victoryTarget = 10)
    {
        MapBaseDefinition defaultBase = FindBaseMap(DefaultMapId) ?? GetFirstAvailableBase();
        if (defaultBase == null)
        {
            Debug.LogError("Unable to create default map. No valid map bases found.");
            return null;
        }

        MapInstance map = new MapInstance(
            Guid.NewGuid().ToString("N"),
            defaultBase,
            MapAffixTier.Common,
            new List<MapRolledAffix>(),
            new List<MapRolledAffix>(),
            new List<MapRolledAffix>(),
            string.Empty,
            string.Empty)
        {
            VictoryConditionType = victoryConditionType,
            VictoryTarget = Mathf.Max(1, victoryTarget),
        };

        return map;
    }

    public static MapInstance CreateDroppedMap(int currentTier, MapDropSettings dropSettings)
    {
        MapDropSettings settings = dropSettings ?? new MapDropSettings();
        DroppedMapAtlasSettings atlasSettings = GetDroppedMapAtlasSettings();
        int targetTier = RollDroppedMapTier(currentTier, settings, atlasSettings);
        MapBaseDefinition baseDefinition = RollBaseMapForTier(targetTier);

        if (baseDefinition == null)
        {
            Debug.LogWarning($"Unable to create dropped map for tier {targetTier}. Falling back to default map.");
            return CreateDefaultMap();
        }

        return CreateGeneratedMap(baseDefinition, atlasSettings.rarityMultiplier, atlasSettings.additionalAffixCount);
    }

    public static OwnedMapRecord CreateOwnedMapRecord(MapInstance map) => MapRecordConverter.CreateRecord(map);

    public static MapInstance CreateMapInstanceFromRecord(OwnedMapRecord record) => MapRecordConverter.CreateInstance(record);

    public static MapBaseDefinition FindBaseMap(string baseMapId)
    {
        MapBaseCatalog baseCatalog = MapCatalogResources.BaseCatalog;
        return baseCatalog != null ? baseCatalog.FindBase(baseMapId) : null;
    }

    public static IReadOnlyList<MapBaseDefinition> GetBaseMaps()
    {
        MapBaseCatalog baseCatalog = MapCatalogResources.BaseCatalog;
        return baseCatalog != null ? baseCatalog.BaseDefinitions : Array.Empty<MapBaseDefinition>();
    }

    public static List<MapBaseDefinition> GetBaseMapsForTier(int tier)
    {
        MapBaseCatalog baseCatalog = MapCatalogResources.BaseCatalog;
        return baseCatalog != null ? baseCatalog.GetValidBases(tier) : new List<MapBaseDefinition>();
    }

    private static MapInstance CreateGeneratedMap(
        MapBaseDefinition baseDefinition,
        float higherRarityMultiplier = 1f,
        int additionalAffixCount = 0)
    {
        if (baseDefinition == null)
        {
            return null;
        }

        MapAffixTier mapRarity = RollMapRarity(higherRarityMultiplier);
        GetAffixStructureForRarity(mapRarity, out int prefixCount, out int suffixCount);

        AffixRollState rollState = new AffixRollState(
            new HashSet<string>(StringComparer.Ordinal),
            new HashSet<MapStatType>());
        List<MapRolledAffix> prefixAffixes = RollAffixes(MapAffixType.Prefix, baseDefinition.Tier, mapRarity, prefixCount, rollState);
        List<MapRolledAffix> suffixAffixes = RollAffixes(MapAffixType.Suffix, baseDefinition.Tier, mapRarity, suffixCount, rollState);
        string displayPrefixAffixName = ChooseDisplayAffixName(prefixAffixes);
        string displaySuffixAffixName = ChooseDisplayAffixName(suffixAffixes);
        List<MapRolledAffix> additionalAffixes = RollAdditionalAffixes(
            baseDefinition.Tier,
            mapRarity,
            additionalAffixCount,
            rollState);

        MapInstance map = new MapInstance(
            Guid.NewGuid().ToString("N"),
            baseDefinition,
            mapRarity,
            prefixAffixes,
            suffixAffixes,
            additionalAffixes,
            displayPrefixAffixName,
            displaySuffixAffixName);
        AssignVictoryCondition(map);
        return map;
    }

    private static List<MapRolledAffix> RollAffixes(
        MapAffixType affixType,
        int mapTier,
        MapAffixTier maxAffixTier,
        int count,
        AffixRollState rollState)
    {
        List<MapRolledAffix> affixes = new List<MapRolledAffix>();
        MapAffixCatalog affixCatalog = MapCatalogResources.AffixCatalog;
        if (affixCatalog == null || count <= 0)
        {
            return affixes;
        }

        for (int i = 0; i < count; i++)
        {
            List<MapAffixDefinition> validAffixes = affixCatalog.GetValidAffixesUpToTier(affixType, maxAffixTier, mapTier);
            RemoveExcludedAffixes(validAffixes, rollState.usedAffixNames, rollState.usedStatTypes);
            if (validAffixes.Count == 0)
            {
                break;
            }

            MapAffixDefinition definition = validAffixes[UnityEngine.Random.Range(0, validAffixes.Count)];
            TrackAffixDefinition(definition, rollState.usedAffixNames, rollState.usedStatTypes);
            affixes.Add(new MapRolledAffix(definition, RollModifiers(definition.Modifiers)));
        }

        return affixes;
    }

    private static List<MapRolledAffix> RollAdditionalAffixes(
        int mapTier,
        MapAffixTier maxAffixTier,
        int additionalAffixCount,
        AffixRollState rollState)
    {
        List<MapRolledAffix> rolledAffixes = new List<MapRolledAffix>();
        MapAffixCatalog affixCatalog = MapCatalogResources.AffixCatalog;

        if (affixCatalog == null || additionalAffixCount <= 0)
        {
            return rolledAffixes;
        }

        for (int i = 0; i < additionalAffixCount; i++)
        {
            List<MapAffixDefinition> validAffixes = GetAllValidAffixesUpToTier(affixCatalog, maxAffixTier, mapTier);
            RemoveExcludedAffixes(validAffixes, rollState.usedAffixNames, rollState.usedStatTypes);

            if (validAffixes.Count == 0)
            {
                break;
            }

            MapAffixDefinition definition = validAffixes[UnityEngine.Random.Range(0, validAffixes.Count)];
            TrackAffixDefinition(definition, rollState.usedAffixNames, rollState.usedStatTypes);
            rolledAffixes.Add(new MapRolledAffix(definition, RollModifiers(definition.Modifiers)));
        }

        return rolledAffixes;
    }

    private static List<MapModifierValue> RollModifiers(IReadOnlyList<MapModifierDefinition> definitions)
    {
        List<MapModifierValue> rolls = new List<MapModifierValue>();
        if (definitions == null)
        {
            return rolls;
        }

        for (int i = 0; i < definitions.Count; i++)
        {
            rolls.Add(new MapModifierValue(definitions[i].statType, definitions[i].Roll()));
        }

        return rolls;
    }

    private static void RemoveExcludedAffixes(
        List<MapAffixDefinition> affixes,
        ISet<string> excludedNames,
        ISet<MapStatType> excludedStatTypes)
    {
        if (affixes == null)
        {
            return;
        }

        affixes.RemoveAll(affix =>
        {
            if (affix == null)
            {
                return true;
            }

            if (excludedNames != null && excludedNames.Contains(affix.AffixName))
            {
                return true;
            }

            if (excludedStatTypes != null)
            {
                for (int i = 0; i < affix.Modifiers.Count; i++)
                {
                    if (excludedStatTypes.Contains(affix.Modifiers[i].statType))
                    {
                        return true;
                    }
                }
            }

            return false;
        });
    }

    private static void TrackAffixDefinition(
        MapAffixDefinition definition,
        ISet<string> usedNames,
        ISet<MapStatType> usedStatTypes)
    {
        if (definition == null)
        {
            return;
        }

        if (usedNames != null && !string.IsNullOrWhiteSpace(definition.AffixName))
        {
            usedNames.Add(definition.AffixName);
        }

        if (usedStatTypes != null)
        {
            for (int i = 0; i < definition.Modifiers.Count; i++)
            {
                usedStatTypes.Add(definition.Modifiers[i].statType);
            }
        }
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

    private static VictoryConditionType RollVictoryConditionType() => UnityEngine.Random.value < 0.5f ? VictoryConditionType.Time : VictoryConditionType.Kills;

    private static int GetTargetByRarity(int[] baseTargets, int tier, MapAffixTier rarity, int perTierValue)
    {
        int rarityIndex = Mathf.Clamp((int)rarity, 0, baseTargets.Length - 1);
        return baseTargets[rarityIndex] + (tier * perTierValue);
    }

    private static MapAffixTier RollMapRarity(float higherRarityMultiplier = 1f)
    {
        float clampedMultiplier = Mathf.Max(0f, higherRarityMultiplier);
        float commonWeight = CommonMapRarityWeight;
        float uncommonWeight = UncommonMapRarityWeight * clampedMultiplier;
        float rareWeight = RareMapRarityWeight * clampedMultiplier;
        float totalWeight = commonWeight + uncommonWeight + rareWeight;

        if (totalWeight <= 0f)
        {
            return MapAffixTier.Common;
        }

        float roll = UnityEngine.Random.Range(0f, totalWeight);

        if (roll < rareWeight)
        {
            return MapAffixTier.Rare;
        }

        if (roll < rareWeight + uncommonWeight)
        {
            return MapAffixTier.Uncommon;
        }

        return MapAffixTier.Common;
    }

    private static void GetAffixStructureForRarity(MapAffixTier rarity, out int prefixCount, out int suffixCount)
    {
        switch (rarity)
        {
            case MapAffixTier.Rare:
                prefixCount = 3;
                suffixCount = 3;
                return;

            case MapAffixTier.Uncommon:
                switch (UnityEngine.Random.Range(0, 3))
                {
                    case 0:
                        prefixCount = 3;
                        suffixCount = 1;
                        return;

                    case 1:
                        prefixCount = 2;
                        suffixCount = 2;
                        return;

                    default:
                        prefixCount = 1;
                        suffixCount = 3;
                        return;
                }

            default:
                prefixCount = 1;
                suffixCount = 1;
                return;
        }
    }

    private static string ChooseDisplayAffixName(IReadOnlyList<MapRolledAffix> affixes)
    {
        if (affixes == null || affixes.Count == 0)
        {
            return string.Empty;
        }

        MapAffixTier highestTier = MapAffixTier.Common;
        for (int i = 0; i < affixes.Count; i++)
        {
            if (affixes[i] != null && affixes[i].AffixTier > highestTier)
            {
                highestTier = affixes[i].AffixTier;
            }
        }

        List<MapRolledAffix> candidates = new List<MapRolledAffix>();
        for (int i = 0; i < affixes.Count; i++)
        {
            if (affixes[i] != null && affixes[i].AffixTier == highestTier)
            {
                candidates.Add(affixes[i]);
            }
        }

        if (candidates.Count == 0)
        {
            return string.Empty;
        }

        return candidates[UnityEngine.Random.Range(0, candidates.Count)].AffixName;
    }

    private static int RollDroppedMapTier(int currentTier, MapDropSettings settings, DroppedMapAtlasSettings atlasSettings)
    {
        if (currentTier <= 0)
        {
            return GetNearestAvailableTierAtOrAbove(1);
        }

        float sameTierWeight = settings.sameTierWeight;
        float aboveTierWeight = settings.aboveTierWeight;
        float belowTierWeight = settings.belowTierWeight;
        ApplyDroppedMapTierAtlasRules(ref sameTierWeight, ref aboveTierWeight, ref belowTierWeight, atlasSettings);

        List<TierWeightOption> candidates = BuildDroppedMapTierCandidates(currentTier, sameTierWeight, aboveTierWeight, belowTierWeight);
        return RollTierFromCandidates(candidates, currentTier);
    }

    private static DroppedMapAtlasSettings GetDroppedMapAtlasSettings()
    {
        return new DroppedMapAtlasSettings(
            1f + (MetaProgressionService.GetAtlasEffectValue(AtlasEffectType.MapRarityPercent) / 100f),
            Mathf.Max(0, Mathf.RoundToInt(MetaProgressionService.GetAtlasEffectValue(AtlasEffectType.AdditionalMapAffixes))),
            Mathf.Max(0f, MetaProgressionService.GetAtlasEffectValue(AtlasEffectType.HigherTierMapDropChancePercent)),
            MetaProgressionService.GetAtlasEffectValue(AtlasEffectType.LowerTierMapsNeverDrop) > 0f);
    }

    private static void ApplyDroppedMapTierAtlasRules(
        ref float sameTierWeight,
        ref float aboveTierWeight,
        ref float belowTierWeight,
        DroppedMapAtlasSettings atlasSettings)
    {
        if (atlasSettings.lowerTierMapsNeverDrop)
        {
            float transferredLowerTierWeight = Mathf.Max(0f, belowTierWeight);
            belowTierWeight = 0f;
            sameTierWeight = Mathf.Max(0f, sameTierWeight + transferredLowerTierWeight);
        }

        if (atlasSettings.higherTierBonus <= 0f)
        {
            return;
        }

        float transferredSameTierWeight = Mathf.Min(atlasSettings.higherTierBonus, Mathf.Max(0f, sameTierWeight));
        sameTierWeight = Mathf.Max(0f, sameTierWeight - transferredSameTierWeight);
        aboveTierWeight = Mathf.Max(0f, aboveTierWeight + transferredSameTierWeight);
    }

    private static List<TierWeightOption> BuildDroppedMapTierCandidates(
        int currentTier,
        float sameTierWeight,
        float aboveTierWeight,
        float belowTierWeight)
    {
        List<TierWeightOption> candidates = new List<TierWeightOption>();
        AddTierWeightCandidate(candidates, currentTier, sameTierWeight);
        AddTierWeightCandidate(candidates, currentTier + 1, aboveTierWeight);

        if (currentTier > 1)
        {
            AddTierWeightCandidate(candidates, currentTier - 1, belowTierWeight);
        }

        return candidates;
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
        for (int i = 0; i < candidates.Count; i++)
        {
            totalWeight += candidates[i].weight;
        }

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float runningWeight = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            runningWeight += candidates[i].weight;
            if (roll <= runningWeight)
            {
                return candidates[i].tier;
            }
        }

        return candidates[candidates.Count - 1].tier;
    }

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

        choicePool.AddRange(GetNonDefaultBaseMaps());
    }

    private static MapBaseDefinition RollBaseMapForTier(int tier) => TakeRandom(GetBaseMapsForTier(tier));

    private static List<MapBaseDefinition> GetNonDefaultBaseMaps()
    {
        IReadOnlyList<MapBaseDefinition> allBases = GetBaseMaps();
        List<MapBaseDefinition> results = new List<MapBaseDefinition>();

        for (int i = 0; i < allBases.Count; i++)
        {
            MapBaseDefinition definition = allBases[i];
            if (IsNonDefaultBaseMap(definition))
            {
                results.Add(definition);
            }
        }

        return results;
    }

    private static bool HasBaseMapsForTier(int tier)
    {
        MapBaseCatalog baseCatalog = MapCatalogResources.BaseCatalog;
        return baseCatalog != null && baseCatalog.HasValidBases(tier);
    }

    private static bool IsNonDefaultBaseMap(MapBaseDefinition baseDefinition)
    {
        return baseDefinition != null && baseDefinition.BaseId != DefaultMapId;
    }

    private static MapBaseDefinition GetFirstAvailableBase()
    {
        IReadOnlyList<MapBaseDefinition> bases = GetBaseMaps();
        for (int i = 0; i < bases.Count; i++)
        {
            if (bases[i] != null && bases[i].IsConfigured())
            {
                return bases[i];
            }
        }

        return null;
    }

    private static T TakeRandom<T>(List<T> source)
    {
        if (source == null || source.Count == 0)
        {
            return default;
        }

        int index = UnityEngine.Random.Range(0, source.Count);
        T value = source[index];
        source.RemoveAt(index);
        return value;
    }

    private static List<MapAffixDefinition> GetAllValidAffixesUpToTier(MapAffixCatalog affixCatalog, MapAffixTier maxAffixTier, int mapTier)
    {
        List<MapAffixDefinition> validAffixes = affixCatalog.GetValidAffixesUpToTier(MapAffixType.Prefix, maxAffixTier, mapTier);
        validAffixes.AddRange(affixCatalog.GetValidAffixesUpToTier(MapAffixType.Suffix, maxAffixTier, mapTier));
        return validAffixes;
    }

    private readonly struct TierWeightOption
    {
        public readonly int tier;
        public readonly float weight;

        public TierWeightOption(int tier, float weight)
        {
            this.tier = tier;
            this.weight = weight;
        }
    }
}

public static class MapRecordConverter
{
    private static readonly IReadOnlyList<MapModifierValue> EmptyRolls = Array.Empty<MapModifierValue>();

    public static OwnedMapRecord CreateRecord(MapInstance map)
    {
        if (map == null || map.BaseDefinition == null)
        {
            return null;
        }

        return new OwnedMapRecord
        {
            instanceId = map.InstanceId,
            baseMapId = map.BaseMapId,
            rarity = map.Rarity,
            prefixAffixes = CreateAffixRecords(map.PrefixAffixes),
            suffixAffixes = CreateAffixRecords(map.SuffixAffixes),
            additionalAffixes = CreateAffixRecords(map.AdditionalAffixes),
            displayPrefixAffixName = map.DisplayPrefixAffixName,
            displaySuffixAffixName = map.DisplaySuffixAffixName,
            victoryConditionType = map.VictoryConditionType,
            victoryTarget = Mathf.Max(1, map.VictoryTarget),
        };
    }

    public static MapInstance CreateInstance(OwnedMapRecord record)
    {
        if (record == null)
        {
            return null;
        }

        MapBaseCatalog baseCatalog = MapCatalogResources.BaseCatalog;
        MapAffixCatalog affixCatalog = MapCatalogResources.AffixCatalog;

        if (baseCatalog == null || affixCatalog == null)
        {
            Debug.LogError("Map catalogs are required to rebuild saved maps.");
            return null;
        }

        MapBaseDefinition baseDefinition = baseCatalog.FindBase(record.baseMapId);
        if (baseDefinition == null)
        {
            Debug.LogWarning($"Unable to rebuild map. Unknown base map id: {record.baseMapId}");
            return null;
        }

        MapInstance map = new MapInstance(
            record.instanceId,
            baseDefinition,
            record.rarity,
            CreateRolledAffixes(record.prefixAffixes, affixCatalog),
            CreateRolledAffixes(record.suffixAffixes, affixCatalog),
            CreateRolledAffixes(record.additionalAffixes, affixCatalog),
            record.displayPrefixAffixName,
            record.displaySuffixAffixName);

        map.VictoryConditionType = record.victoryConditionType;
        map.VictoryTarget = Mathf.Max(1, record.victoryTarget);
        return map;
    }

    private static OwnedMapAffixRecord CreateAffixRecord(MapRolledAffix affix)
    {
        if (affix?.AffixDefinition == null)
        {
            return null;
        }

        return new OwnedMapAffixRecord
        {
            affixName = affix.AffixName,
            modifierRolls = CopyRolls(affix.ModifierRolls),
        };
    }

    private static List<OwnedMapAffixRecord> CreateAffixRecords(IReadOnlyList<MapRolledAffix> affixes)
    {
        List<OwnedMapAffixRecord> records = new List<OwnedMapAffixRecord>();

        if (affixes == null)
        {
            return records;
        }

        for (int i = 0; i < affixes.Count; i++)
        {
            OwnedMapAffixRecord record = CreateAffixRecord(affixes[i]);
            if (record != null)
            {
                records.Add(record);
            }
        }

        return records;
    }

    private static List<MapRolledAffix> CreateRolledAffixes(List<OwnedMapAffixRecord> records, MapAffixCatalog affixCatalog)
    {
        List<MapRolledAffix> affixes = new List<MapRolledAffix>();

        if (records == null)
        {
            return affixes;
        }

        for (int i = 0; i < records.Count; i++)
        {
            OwnedMapAffixRecord record = records[i];
            if (record == null || string.IsNullOrWhiteSpace(record.affixName) || affixCatalog == null)
            {
                continue;
            }

            MapAffixDefinition definition = affixCatalog.FindAffix(record.affixName);
            if (definition == null)
            {
                continue;
            }

            MapRolledAffix affix = new MapRolledAffix(definition, CopyRolls(record.modifierRolls));
            if (affix != null)
            {
                affixes.Add(affix);
            }
        }

        return affixes;
    }

    private static List<MapModifierValue> CopyRolls(IReadOnlyList<MapModifierValue> rolls) =>
        new(rolls ?? EmptyRolls);
}
