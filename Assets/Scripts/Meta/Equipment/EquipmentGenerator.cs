using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EquipmentGenerator
{
    public const float DefaultCommonWeight = 60f;
    public const float DefaultUncommonWeight = 30f;
    public const float DefaultRareWeight = 10f;

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
        int itemTier)
    {
        EquipmentGenerationRequest request = new EquipmentGenerationRequest
        {
            minItemTier = itemTier,
            maxItemTier = itemTier,
            forceSlotType = true,
            forcedSlotType = slotType,
        };

        return Generate(baseCatalog, affixCatalog, request);
    }

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
            Debug.LogWarning($"No valid equipment bases found for slot {slotType} at tier {itemTier}.");
            return null;
        }

        EquipmentBaseDefinition baseDefinition = validBases[Random.Range(0, validBases.Count)];
        List<EquipmentModifierRoll> implicitRolls = RollModifiers(baseDefinition.ImplicitModifiers, itemTier);

        List<EquipmentRolledAffix> prefixAffixes = new List<EquipmentRolledAffix>();
        List<EquipmentRolledAffix> suffixAffixes = new List<EquipmentRolledAffix>();

        switch (rarity)
        {
            case EquipmentRarity.Common:
                RollCommonAffix(affixCatalog, request, slotType, itemTier, itemLevel, prefixAffixes, suffixAffixes);
                break;

            case EquipmentRarity.Uncommon:
                if (!TryRollFixedAffixes(affixCatalog, request, slotType, itemTier, itemLevel, 1, 1, out prefixAffixes, out suffixAffixes))
                {
                    Debug.LogWarning($"Unable to roll both affixes for an {rarity} item at tier {itemTier} in slot {slotType}.");
                    return null;
                }
                break;

            case EquipmentRarity.Rare:
                if (!TryRollRareAffixes(affixCatalog, request, slotType, itemTier, itemLevel, out prefixAffixes, out suffixAffixes))
                {
                    Debug.LogWarning($"Unable to roll rare affixes for a {rarity} item at tier {itemTier} in slot {slotType}.");
                    return null;
                }
                break;
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

    public static EquipmentModifierRoll RollModifier(EquipmentModifierDefinition modifier, int itemTier)
    {
        float scaledMin = ApplyTierScaling(modifier.minValue, modifier.tierScalingMode, modifier.tierScalingAmount, itemTier);
        float scaledMax = ApplyTierScaling(modifier.maxValue, modifier.tierScalingMode, modifier.tierScalingAmount, itemTier);
        float value = Mathf.Approximately(scaledMin, scaledMax)
            ? scaledMin
            : Random.Range(Mathf.Min(scaledMin, scaledMax), Mathf.Max(scaledMin, scaledMax));

        return new EquipmentModifierRoll(modifier.statType, modifier.modifierKind, value);
    }

    private static void RollCommonAffix(
        EquipmentAffixCatalog affixCatalog,
        EquipmentGenerationRequest request,
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        List<EquipmentRolledAffix> prefixAffixes,
        List<EquipmentRolledAffix> suffixAffixes)
    {
        HashSet<string> usedTags = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        bool rollPrefix = Random.value < 0.5f;
        EquipmentAffixType affixType = rollPrefix ? EquipmentAffixType.Prefix : EquipmentAffixType.Suffix;
        EquipmentAffixDefinition affix = RollAffix(affixCatalog, affixType, slotType, itemTier, itemLevel, request.requiredAffixStats, usedTags);

        if (affix == null)
        {
            affixType = rollPrefix ? EquipmentAffixType.Suffix : EquipmentAffixType.Prefix;
            affix = RollAffix(affixCatalog, affixType, slotType, itemTier, itemLevel, request.requiredAffixStats, usedTags);
        }

        if (affix == null)
        {
            return;
        }

        if (affixType == EquipmentAffixType.Prefix)
        {
            prefixAffixes.Add(CreateRolledAffix(affix, itemTier));
        }
        else
        {
            suffixAffixes.Add(CreateRolledAffix(affix, itemTier));
        }
    }

    private static bool TryRollFixedAffixes(
        EquipmentAffixCatalog affixCatalog,
        EquipmentGenerationRequest request,
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        int requiredPrefixCount,
        int requiredSuffixCount,
        out List<EquipmentRolledAffix> prefixAffixes,
        out List<EquipmentRolledAffix> suffixAffixes)
    {
        HashSet<string> usedTags = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
        prefixAffixes = RollAffixSet(affixCatalog, EquipmentAffixType.Prefix, slotType, itemTier, itemLevel, request.requiredAffixStats, requiredPrefixCount, usedTags);
        suffixAffixes = RollAffixSet(affixCatalog, EquipmentAffixType.Suffix, slotType, itemTier, itemLevel, request.requiredAffixStats, requiredSuffixCount, usedTags);
        return prefixAffixes != null && suffixAffixes != null;
    }

    private static bool TryRollRareAffixes(
        EquipmentAffixCatalog affixCatalog,
        EquipmentGenerationRequest request,
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        out List<EquipmentRolledAffix> prefixAffixes,
        out List<EquipmentRolledAffix> suffixAffixes)
    {
        prefixAffixes = null;
        suffixAffixes = null;

        (int prefixes, int suffixes)[] combinations = new[]
        {
            (2, 2),
            (2, 1),
            (1, 2),
        };

        Shuffle(combinations);

        for (int i = 0; i < combinations.Length; i++)
        {
            if (TryRollFixedAffixes(
                affixCatalog,
                request,
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
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        IReadOnlyList<EquipmentStatType> requiredStats,
        int count,
        ISet<string> usedTags)
    {
        List<EquipmentAffixDefinition> validAffixes = affixCatalog.GetValidAffixes(affixType, slotType, itemTier, itemLevel);
        RemoveBlockedAffixes(validAffixes, usedTags);

        if (requiredStats != null && requiredStats.Count > 0)
        {
            validAffixes.RemoveAll(affix => !MatchesRequiredStats(affix, requiredStats));
        }

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
        EquipmentSlotType slotType,
        int itemTier,
        int itemLevel,
        IReadOnlyList<EquipmentStatType> requiredStats,
        ISet<string> usedTags)
    {
        List<EquipmentAffixDefinition> validAffixes = affixCatalog.GetValidAffixes(affixType, slotType, itemTier, itemLevel);
        RemoveBlockedAffixes(validAffixes, usedTags);

        if (requiredStats != null && requiredStats.Count > 0)
        {
            validAffixes.RemoveAll(affix => !MatchesRequiredStats(affix, requiredStats));
        }

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

    private static bool MatchesRequiredStats(EquipmentAffixDefinition affix, IReadOnlyList<EquipmentStatType> requiredStats)
    {
        foreach (EquipmentModifierDefinition modifier in affix.Modifiers)
        {
            if (requiredStats.Contains(modifier.statType))
            {
                return true;
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

        List<EquipmentSlotType> validSlotTypes = new List<EquipmentSlotType>();

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
        if (baseCatalog.GetValidBases(slotType, itemTier).Count == 0)
        {
            return false;
        }

        List<EquipmentAffixDefinition> validPrefixes = affixCatalog.GetValidAffixes(
            EquipmentAffixType.Prefix,
            slotType,
            itemTier,
            itemLevel);
        List<EquipmentAffixDefinition> validSuffixes = affixCatalog.GetValidAffixes(
            EquipmentAffixType.Suffix,
            slotType,
            itemTier,
            itemLevel);

        if (request.requiredAffixStats != null && request.requiredAffixStats.Count > 0)
        {
            validPrefixes.RemoveAll(affix => !MatchesRequiredStats(affix, request.requiredAffixStats));
            validSuffixes.RemoveAll(affix => !MatchesRequiredStats(affix, request.requiredAffixStats));
        }

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
        HashSet<string> usedTags = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
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
        List<EquipmentModifierRoll> rolls = new List<EquipmentModifierRoll>();

        if (modifiers == null)
        {
            return rolls;
        }

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
}
