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

        EquipmentAffixDefinition prefixAffix = null;
        EquipmentAffixDefinition suffixAffix = null;
        List<EquipmentModifierRoll> prefixRolls = new List<EquipmentModifierRoll>();
        List<EquipmentModifierRoll> suffixRolls = new List<EquipmentModifierRoll>();

        switch (rarity)
        {
            case EquipmentRarity.Common:
                RollCommonAffix(affixCatalog, request, slotType, itemTier, out prefixAffix, out suffixAffix, prefixRolls, suffixRolls);
                break;

            case EquipmentRarity.Uncommon:
            case EquipmentRarity.Rare:
                prefixAffix = RollAffix(affixCatalog, EquipmentAffixType.Prefix, slotType, itemTier, request.requiredAffixStats);
                suffixAffix = RollAffix(affixCatalog, EquipmentAffixType.Suffix, slotType, itemTier, request.requiredAffixStats);
                if (prefixAffix == null || suffixAffix == null)
                {
                    Debug.LogWarning($"Unable to roll both affixes for a {rarity} item at tier {itemTier} in slot {slotType}.");
                    return null;
                }

                prefixRolls = RollModifiers(prefixAffix.Modifiers, itemTier);
                suffixRolls = RollModifiers(suffixAffix.Modifiers, itemTier);
                break;
        }

        return new EquipmentInstance(
            rarity,
            itemTier,
            baseDefinition,
            prefixAffix,
            suffixAffix,
            implicitRolls,
            prefixRolls,
            suffixRolls);
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
        out EquipmentAffixDefinition prefixAffix,
        out EquipmentAffixDefinition suffixAffix,
        List<EquipmentModifierRoll> prefixRolls,
        List<EquipmentModifierRoll> suffixRolls)
    {
        prefixAffix = null;
        suffixAffix = null;

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
            prefixAffix = affix;
            prefixRolls.AddRange(RollModifiers(affix.Modifiers, itemTier));
        }
        else
        {
            suffixAffix = affix;
            suffixRolls.AddRange(RollModifiers(affix.Modifiers, itemTier));
        }
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
            EquipmentRarity.Rare => validPrefixes.Count > 0 && validSuffixes.Count > 0,
            _ => false
        };
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
