using System.Collections.Generic;
using UnityEngine;

public static class EquipmentGenerator
{
    public const float DefaultCommonWeight = 60f;
    public const float DefaultUncommonWeight = 30f;
    public const float DefaultRareWeight = 10f;
    private static readonly System.StringComparer TagComparer = System.StringComparer.OrdinalIgnoreCase;

    private static readonly (int prefixes, int suffixes)[] RareAffixCombinations =
    {
        (2, 2),
        (2, 1),
        (1, 2),
    };

    public static EquipmentInstance Generate(
        EquipmentBaseCatalog baseCatalog,
        EquipmentAffixCatalog affixCatalog,
        EquipmentGenerationRequest request)
    {
        return Generate(
            baseCatalog,
            affixCatalog,
            request,
            DefaultCommonWeight,
            DefaultUncommonWeight,
            DefaultRareWeight);
    }

    public static EquipmentInstance GenerateForSlot(
        EquipmentBaseCatalog baseCatalog,
        EquipmentAffixCatalog affixCatalog,
        EquipmentSlotType slotType,
        int itemTier) =>
        Generate(baseCatalog, affixCatalog, new EquipmentGenerationRequest
        {
            minItemTier = itemTier,
            maxItemTier = itemTier,
            forceSlotType = true,
            forcedSlotType = slotType,
        });

    public static EquipmentInstance Generate(
        EquipmentBaseCatalog baseCatalog,
        EquipmentAffixCatalog affixCatalog,
        EquipmentGenerationRequest request,
        float commonWeight,
        float uncommonWeight,
        float rareWeight)
    {
        if (baseCatalog == null || affixCatalog == null || request == null)
        {
            return null;
        }

        int itemTier = Random.Range(request.GetClampedMinTier(), request.GetClampedMaxTier() + 1);
        int itemLevel = request.GetClampedItemLevel();
        EquipmentRarity rarity = RollRarity(commonWeight, uncommonWeight, rareWeight);
        EquipmentSlotType slotType = ResolveSlotType(baseCatalog, affixCatalog, request, itemTier, itemLevel, rarity);
        List<EquipmentBaseDefinition> validBases = baseCatalog.GetValidBases(slotType, itemTier);
        if (validBases.Count == 0)
        {
            WarnMissingBases(slotType, itemTier);
            return null;
        }

        EquipmentBaseDefinition baseDefinition = validBases[Random.Range(0, validBases.Count)];
        List<EquipmentModifierRoll> implicitRolls = RollModifiers(baseDefinition.ImplicitModifiers, itemTier);
        if (!TryRollAffixes(affixCatalog, request, baseDefinition, slotType, itemTier, itemLevel, rarity, out List<EquipmentRolledAffix> prefixAffixes, out List<EquipmentRolledAffix> suffixAffixes))
        {
            WarnMissingAffixes(rarity, slotType, itemTier);
            return null;
        }

        return new EquipmentInstance(
            rarity,
            itemTier,
            itemLevel,
            baseDefinition,
            implicitRolls,
            prefixAffixes,
            suffixAffixes);
    }

    // Each rarity follows a different affix budget, but they all collapse to the same pair of output lists.
    private static bool TryRollAffixes(
        EquipmentAffixCatalog affixCatalog,
        EquipmentGenerationRequest request,
        EquipmentBaseDefinition baseDefinition,
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        EquipmentRarity rarity,
        out List<EquipmentRolledAffix> prefixAffixes,
        out List<EquipmentRolledAffix> suffixAffixes)
    {
        prefixAffixes = new();
        suffixAffixes = new();

        return rarity switch
        {
            EquipmentRarity.Common => RollCommonAffix(affixCatalog, request, baseDefinition, slotType, itemTier, itemLevel, prefixAffixes, suffixAffixes),
            EquipmentRarity.Uncommon => TryRollFixedAffixes(affixCatalog, request, baseDefinition, slotType, itemTier, itemLevel, 1, 1, out prefixAffixes, out suffixAffixes),
            EquipmentRarity.Rare => TryRollRareAffixes(affixCatalog, request, baseDefinition, slotType, itemTier, itemLevel, out prefixAffixes, out suffixAffixes),
            _ => false,
        };
    }

    public static EquipmentModifierRoll RollModifier(EquipmentModifierDefinition modifier, int itemTier)
    {
        float scaledMin = ApplyTierScaling(modifier.minValue, modifier.tierScalingMode, modifier.tierScalingAmount, itemTier);
        float scaledMax = ApplyTierScaling(modifier.maxValue, modifier.tierScalingMode, modifier.tierScalingAmount, itemTier);
        float value = Mathf.Approximately(scaledMin, scaledMax)
            ? scaledMin
            : Random.Range(Mathf.Min(scaledMin, scaledMax), Mathf.Max(scaledMin, scaledMax));

        return new EquipmentModifierRoll(modifier.statType, modifier.modifierKind, value);
    }

    private static bool RollCommonAffix(
        EquipmentAffixCatalog affixCatalog,
        EquipmentGenerationRequest request,
        EquipmentBaseDefinition baseDefinition,
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        List<EquipmentRolledAffix> prefixAffixes,
        List<EquipmentRolledAffix> suffixAffixes)
    {
        HashSet<string> usedTags = new(TagComparer);
        bool rollPrefix = Random.value < 0.5f;
        EquipmentAffixType affixType = rollPrefix ? EquipmentAffixType.Prefix : EquipmentAffixType.Suffix;
        EquipmentAffixDefinition affix = RollAffix(affixCatalog, affixType, baseDefinition, slotType, itemTier, itemLevel, request.requiredAffixStats, usedTags);

        if (affix == null)
        {
            affixType = rollPrefix ? EquipmentAffixType.Suffix : EquipmentAffixType.Prefix;
            affix = RollAffix(affixCatalog, affixType, baseDefinition, slotType, itemTier, itemLevel, request.requiredAffixStats, usedTags);
        }

        if (affix == null)
        {
            return true;
        }

        GetRolledAffixList(affixType, prefixAffixes, suffixAffixes).Add(CreateRolledAffix(affix, itemTier));
        return true;
    }

    private static bool TryRollFixedAffixes(
        EquipmentAffixCatalog affixCatalog,
        EquipmentGenerationRequest request,
        EquipmentBaseDefinition baseDefinition,
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        int requiredPrefixCount,
        int requiredSuffixCount,
        out List<EquipmentRolledAffix> prefixAffixes,
        out List<EquipmentRolledAffix> suffixAffixes)
    {
        HashSet<string> usedTags = new(TagComparer);
        prefixAffixes = RollAffixSet(affixCatalog, EquipmentAffixType.Prefix, baseDefinition, slotType, itemTier, itemLevel, request.requiredAffixStats, requiredPrefixCount, usedTags);
        suffixAffixes = RollAffixSet(affixCatalog, EquipmentAffixType.Suffix, baseDefinition, slotType, itemTier, itemLevel, request.requiredAffixStats, requiredSuffixCount, usedTags);
        return prefixAffixes != null && suffixAffixes != null;
    }

    private static bool TryRollRareAffixes(
        EquipmentAffixCatalog affixCatalog,
        EquipmentGenerationRequest request,
        EquipmentBaseDefinition baseDefinition,
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        out List<EquipmentRolledAffix> prefixAffixes,
        out List<EquipmentRolledAffix> suffixAffixes)
    {
        prefixAffixes = null;
        suffixAffixes = null;

        var combinations = new (int prefixes, int suffixes)[RareAffixCombinations.Length];
        RareAffixCombinations.CopyTo(combinations, 0);
        Shuffle(combinations);

        for (int i = 0; i < combinations.Length; i++)
        {
                if (TryRollFixedAffixes(
                affixCatalog,
                request,
                baseDefinition,
                slotType,
                itemTier,
                itemLevel,
                combinations[i].prefixes,
                combinations[i].suffixes,
                out prefixAffixes,
                out suffixAffixes))
            {
                return true;
            }
        }

        return false;
    }

    private static List<EquipmentRolledAffix> RollAffixSet(
        EquipmentAffixCatalog affixCatalog,
        EquipmentAffixType affixType,
        EquipmentBaseDefinition baseDefinition,
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        IReadOnlyList<EquipmentStatType> requiredStats,
        int count,
        ISet<string> usedTags)
    {
        List<EquipmentAffixDefinition> validAffixes = GetFilteredAffixes(affixCatalog, affixType, baseDefinition, slotType, itemTier, itemLevel, requiredStats, usedTags);
        if (validAffixes.Count < count)
        {
            return null;
        }

        List<EquipmentRolledAffix> rolledAffixes = new List<EquipmentRolledAffix>();

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, validAffixes.Count);
            EquipmentAffixDefinition affix = validAffixes[randomIndex];
            validAffixes.RemoveAt(randomIndex);
            TrackAffixTag(affix, usedTags);
            RemoveBlockedAffixes(validAffixes, usedTags);
            rolledAffixes.Add(CreateRolledAffix(affix, itemTier));

            if (validAffixes.Count + rolledAffixes.Count < count)
            {
                return null;
            }
        }

        return rolledAffixes;
    }

    private static EquipmentAffixDefinition RollAffix(
        EquipmentAffixCatalog affixCatalog,
        EquipmentAffixType affixType,
        EquipmentBaseDefinition baseDefinition,
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        IReadOnlyList<EquipmentStatType> requiredStats,
        ISet<string> usedTags)
    {
        List<EquipmentAffixDefinition> validAffixes = GetFilteredAffixes(affixCatalog, affixType, baseDefinition, slotType, itemTier, itemLevel, requiredStats, usedTags);
        if (validAffixes.Count == 0)
        {
            return null;
        }

        EquipmentAffixDefinition affix = validAffixes[Random.Range(0, validAffixes.Count)];
        TrackAffixTag(affix, usedTags);
        return affix;
    }

    private static EquipmentRolledAffix CreateRolledAffix(EquipmentAffixDefinition affix, int itemTier)
    {
        return affix == null ? null : new EquipmentRolledAffix(affix, RollModifiers(affix.Modifiers, itemTier));
    }

    private static List<EquipmentAffixDefinition> GetFilteredAffixes(
        EquipmentAffixCatalog affixCatalog,
        EquipmentAffixType affixType,
        EquipmentBaseDefinition baseDefinition,
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        IReadOnlyList<EquipmentStatType> requiredStats,
        ISet<string> usedTags = null)
    {
        List<EquipmentAffixDefinition> affixes = affixCatalog.GetValidAffixes(affixType, slotType, itemTier, itemLevel);
        FilterLocalDefenseAffixes(affixes, baseDefinition);
        RemoveBlockedAffixes(affixes, usedTags);
        FilterRequiredStats(affixes, requiredStats);
        return affixes;
    }

    private static bool MatchesRequiredStats(EquipmentAffixDefinition affix, IReadOnlyList<EquipmentStatType> requiredStats)
    {
        if (affix == null || requiredStats == null || requiredStats.Count == 0)
        {
            return false;
        }

        foreach (EquipmentModifierDefinition modifier in affix.Modifiers)
        {
            for (int i = 0; i < requiredStats.Count; i++)
            {
                if (requiredStats[i] == modifier.statType)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static EquipmentSlotType ResolveSlotType(
        EquipmentBaseCatalog baseCatalog,
        EquipmentAffixCatalog affixCatalog,
        EquipmentGenerationRequest request,
        int itemTier,
        int itemLevel,
        EquipmentRarity rarity)
    {
        if (request.forceSlotType)
        {
            return request.forcedSlotType;
        }

        List<EquipmentSlotType> validSlotTypes = new();
        foreach (EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentSlotType)))
        {
            if (CanGenerateForSlot(baseCatalog, affixCatalog, request, slotType, itemTier, itemLevel, rarity))
            {
                validSlotTypes.Add(slotType);
            }
        }

        if (validSlotTypes.Count == 0)
        {
            return EquipmentSlotType.Head;
        }

        return validSlotTypes[Random.Range(0, validSlotTypes.Count)];
    }

    private static bool CanGenerateForSlot(
        EquipmentBaseCatalog baseCatalog,
        EquipmentAffixCatalog affixCatalog,
        EquipmentGenerationRequest request,
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        EquipmentRarity rarity)
    {
        List<EquipmentBaseDefinition> validBases = baseCatalog.GetValidBases(slotType, itemTier);
        if (validBases.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < validBases.Count; i++)
        {
            if (CanGenerateForBase(affixCatalog, request, validBases[i], slotType, itemTier, itemLevel, rarity))
            {
                return true;
            }
        }

        return false;
    }

    private static bool CanGenerateForBase(
        EquipmentAffixCatalog affixCatalog,
        EquipmentGenerationRequest request,
        EquipmentBaseDefinition baseDefinition,
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        EquipmentRarity rarity)
    {
        List<EquipmentAffixDefinition> validPrefixes = GetFilteredAffixes(affixCatalog, EquipmentAffixType.Prefix, baseDefinition, slotType, itemTier, itemLevel, request.requiredAffixStats);
        List<EquipmentAffixDefinition> validSuffixes = GetFilteredAffixes(affixCatalog, EquipmentAffixType.Suffix, baseDefinition, slotType, itemTier, itemLevel, request.requiredAffixStats);

        return rarity switch
        {
            EquipmentRarity.Common => validPrefixes.Count > 0 || validSuffixes.Count > 0,
            EquipmentRarity.Uncommon => CanRollUniqueTagCombination(validPrefixes, validSuffixes, 1, 1),
            EquipmentRarity.Rare => CanRollUniqueTagCombination(validPrefixes, validSuffixes, 2, 1)
                || CanRollUniqueTagCombination(validPrefixes, validSuffixes, 1, 2),
            _ => false
        };
    }

    // Affixes with the same tag are treated as the same family and cannot coexist on one item.
    private static void RemoveBlockedAffixes(List<EquipmentAffixDefinition> affixes, ISet<string> usedTags)
    {
        if (affixes == null || usedTags == null || usedTags.Count == 0)
        {
            return;
        }

        affixes.RemoveAll(affix => IsBlockedByTag(affix, usedTags));
    }

    private static bool IsBlockedByTag(EquipmentAffixDefinition affix, ISet<string> usedTags)
    {
        return affix != null
            && usedTags != null
            && !string.IsNullOrWhiteSpace(affix.AffixTag)
            && usedTags.Contains(affix.AffixTag);
    }

    private static void TrackAffixTag(EquipmentAffixDefinition affix, ISet<string> usedTags)
    {
        if (affix == null || usedTags == null || string.IsNullOrWhiteSpace(affix.AffixTag))
        {
            return;
        }

        usedTags.Add(affix.AffixTag);
    }

    private static bool CanRollUniqueTagCombination(
        IReadOnlyList<EquipmentAffixDefinition> validPrefixes,
        IReadOnlyList<EquipmentAffixDefinition> validSuffixes,
        int requiredPrefixCount,
        int requiredSuffixCount)
    {
        HashSet<string> usedTags = new(TagComparer);
        int resolvedPrefixes = ReserveUniqueTags(validPrefixes, requiredPrefixCount, usedTags);
        int resolvedSuffixes = ReserveUniqueTags(validSuffixes, requiredSuffixCount, usedTags);
        return resolvedPrefixes >= requiredPrefixCount && resolvedSuffixes >= requiredSuffixCount;
    }

    private static int ReserveUniqueTags(
        IReadOnlyList<EquipmentAffixDefinition> affixes,
        int requiredCount,
        ISet<string> usedTags)
    {
        int reservedCount = 0;

        if (affixes == null)
        {
            return reservedCount;
        }

        for (int i = 0; i < affixes.Count && reservedCount < requiredCount; i++)
        {
            EquipmentAffixDefinition affix = affixes[i];
            if (affix == null || IsBlockedByTag(affix, usedTags))
            {
                continue;
            }

            TrackAffixTag(affix, usedTags);
            reservedCount++;
        }

        return reservedCount;
    }

    private static void FilterRequiredStats(List<EquipmentAffixDefinition> affixes, IReadOnlyList<EquipmentStatType> requiredStats)
    {
        if (affixes == null || requiredStats == null || requiredStats.Count == 0)
        {
            return;
        }

        affixes.RemoveAll(affix => !MatchesRequiredStats(affix, requiredStats));
    }

    private static void FilterLocalDefenseAffixes(List<EquipmentAffixDefinition> affixes, EquipmentBaseDefinition baseDefinition)
    {
        if (affixes == null)
        {
            return;
        }

        affixes.RemoveAll(affix => !EquipmentLocalDefenseUtility.AffixMatchesBaseImplicit(affix, baseDefinition));
    }

    private static void Shuffle<T>(T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int randomIndex = Random.Range(i, array.Length);
            (array[i], array[randomIndex]) = (array[randomIndex], array[i]);
        }
    }

    private static List<EquipmentModifierRoll> RollModifiers(IReadOnlyList<EquipmentModifierDefinition> modifiers, int itemTier)
    {
        List<EquipmentModifierRoll> rolls = new();
        if (modifiers == null) return rolls;

        for (int i = 0; i < modifiers.Count; i++)
        {
            rolls.Add(RollModifier(modifiers[i], itemTier));
        }

        return rolls;
    }

    private static float ApplyTierScaling(float baseValue, EquipmentTierScalingMode scalingMode, float scalingAmount, int itemTier)
    {
        int tierOffset = Mathf.Max(0, itemTier - 1);

        return scalingMode switch
        {
            EquipmentTierScalingMode.None => baseValue,
            EquipmentTierScalingMode.FlatPerTier => baseValue + (scalingAmount * tierOffset),
            EquipmentTierScalingMode.PercentPerTier => baseValue * (1f + (scalingAmount * tierOffset)),
            _ => baseValue
        };
    }

    private static EquipmentRarity RollRarity(float commonWeight, float uncommonWeight, float rareWeight)
    {
        commonWeight = Mathf.Max(0f, commonWeight);
        uncommonWeight = Mathf.Max(0f, uncommonWeight);
        rareWeight = Mathf.Max(0f, rareWeight);

        float totalWeight = commonWeight + uncommonWeight + rareWeight;

        if (totalWeight <= 0f)
        {
            return EquipmentRarity.Common;
        }

        float roll = Random.Range(0f, totalWeight);

        if (roll < commonWeight)
        {
            return EquipmentRarity.Common;
        }

        if (roll < commonWeight + uncommonWeight)
        {
            return EquipmentRarity.Uncommon;
        }

        return EquipmentRarity.Rare;
    }

    private static List<EquipmentRolledAffix> GetRolledAffixList(
        EquipmentAffixType affixType,
        List<EquipmentRolledAffix> prefixAffixes,
        List<EquipmentRolledAffix> suffixAffixes) =>
        affixType == EquipmentAffixType.Prefix ? prefixAffixes : suffixAffixes;

    private static void WarnMissingBases(EquipmentSlotType slotType, int itemTier) =>
        Debug.LogWarning($"No valid equipment bases found for slot {slotType} at tier {itemTier}.");

    private static void WarnMissingAffixes(EquipmentRarity rarity, EquipmentSlotType slotType, int itemTier) =>
        Debug.LogWarning($"Unable to roll affixes for a {rarity} item at tier {itemTier} in slot {slotType}.");
}
