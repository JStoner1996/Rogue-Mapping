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
        EquipmentRarity rarity = RollRarity(commonWeight, uncommonWeight, rareWeight);
        EquipmentSlotType slotType = ResolveSlotType(baseCatalog, affixCatalog, request, itemTier, rarity);
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
                RollCommonAffix(affixCatalog, request, slotType, itemTier, prefixAffixes, suffixAffixes);
                break;

            case EquipmentRarity.Uncommon:
                if (!TryRollFixedAffixes(affixCatalog, request, slotType, itemTier, 1, 1, out prefixAffixes, out suffixAffixes))
                {
                    Debug.LogWarning($"Unable to roll both affixes for an {rarity} item at tier {itemTier} in slot {slotType}.");
                    return null;
                }
                break;

            case EquipmentRarity.Rare:
                if (!TryRollRareAffixes(affixCatalog, request, slotType, itemTier, out prefixAffixes, out suffixAffixes))
                {
                    Debug.LogWarning($"Unable to roll rare affixes for a {rarity} item at tier {itemTier} in slot {slotType}.");
                    return null;
                }
                break;
        }

        return new EquipmentInstance(
            rarity,
            itemTier,
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
        List<EquipmentRolledAffix> prefixAffixes,
        List<EquipmentRolledAffix> suffixAffixes)
    {
        bool rollPrefix = Random.value < 0.5f;
        EquipmentAffixType affixType = rollPrefix ? EquipmentAffixType.Prefix : EquipmentAffixType.Suffix;
        EquipmentAffixDefinition affix = RollAffix(affixCatalog, affixType, slotType, itemTier, request.requiredAffixStats);

        if (affix == null)
        {
            affixType = rollPrefix ? EquipmentAffixType.Suffix : EquipmentAffixType.Prefix;
            affix = RollAffix(affixCatalog, affixType, slotType, itemTier, request.requiredAffixStats);
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
        int requiredPrefixCount,
        int requiredSuffixCount,
        out List<EquipmentRolledAffix> prefixAffixes,
        out List<EquipmentRolledAffix> suffixAffixes)
    {
        prefixAffixes = RollAffixSet(affixCatalog, EquipmentAffixType.Prefix, slotType, itemTier, request.requiredAffixStats, requiredPrefixCount);
        suffixAffixes = RollAffixSet(affixCatalog, EquipmentAffixType.Suffix, slotType, itemTier, request.requiredAffixStats, requiredSuffixCount);
        return prefixAffixes != null && suffixAffixes != null;
    }

    private static bool TryRollRareAffixes(
        EquipmentAffixCatalog affixCatalog,
        EquipmentGenerationRequest request,
        EquipmentSlotType slotType,
        int itemTier,
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
        IReadOnlyList<EquipmentStatType> requiredStats,
        int count)
    {
        List<EquipmentAffixDefinition> validAffixes = affixCatalog.GetValidAffixes(affixType, slotType, itemTier);

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
            rolledAffixes.Add(CreateRolledAffix(affix, itemTier));
        }

        return rolledAffixes;
    }

    private static EquipmentAffixDefinition RollAffix(
        EquipmentAffixCatalog affixCatalog,
        EquipmentAffixType affixType,
        EquipmentSlotType slotType,
        int itemTier,
        IReadOnlyList<EquipmentStatType> requiredStats)
    {
        List<EquipmentAffixDefinition> validAffixes = affixCatalog.GetValidAffixes(affixType, slotType, itemTier);

        if (requiredStats != null && requiredStats.Count > 0)
        {
            validAffixes.RemoveAll(affix => !MatchesRequiredStats(affix, requiredStats));
        }

        if (validAffixes.Count == 0)
        {
            return null;
        }

        return validAffixes[Random.Range(0, validAffixes.Count)];
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
        EquipmentRarity rarity)
    {
        if (request.forceSlotType)
        {
            return request.forcedSlotType;
        }

        List<EquipmentSlotType> validSlotTypes = new List<EquipmentSlotType>();

        foreach (EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentSlotType)))
        {
            if (CanGenerateForSlot(baseCatalog, affixCatalog, request, slotType, itemTier, rarity))
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
        EquipmentRarity rarity)
    {
        if (baseCatalog.GetValidBases(slotType, itemTier).Count == 0)
        {
            return false;
        }

        List<EquipmentAffixDefinition> validPrefixes = affixCatalog.GetValidAffixes(
            EquipmentAffixType.Prefix,
            slotType,
            itemTier);
        List<EquipmentAffixDefinition> validSuffixes = affixCatalog.GetValidAffixes(
            EquipmentAffixType.Suffix,
            slotType,
            itemTier);

        if (request.requiredAffixStats != null && request.requiredAffixStats.Count > 0)
        {
            validPrefixes.RemoveAll(affix => !MatchesRequiredStats(affix, request.requiredAffixStats));
            validSuffixes.RemoveAll(affix => !MatchesRequiredStats(affix, request.requiredAffixStats));
        }

        return rarity switch
        {
            EquipmentRarity.Common => validPrefixes.Count > 0 || validSuffixes.Count > 0,
            EquipmentRarity.Uncommon => validPrefixes.Count > 0 && validSuffixes.Count > 0,
            EquipmentRarity.Rare => (validPrefixes.Count >= 2 && validSuffixes.Count >= 1)
                || (validPrefixes.Count >= 1 && validSuffixes.Count >= 2),
            _ => false
        };
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
