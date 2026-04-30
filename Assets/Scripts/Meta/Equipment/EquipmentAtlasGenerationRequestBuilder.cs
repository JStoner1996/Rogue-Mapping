using UnityEngine;

public static class EquipmentAtlasGenerationRequestBuilder
{
    public static EquipmentGenerationRequest Build(
        EquipmentDropSettings settings,
        MapInstance selectedMap,
        EnemyArchetypeDefinition archetypeDefinition)
    {
        EquipmentGenerationRequest request = (settings ?? new EquipmentDropSettings()).BuildRequest(selectedMap);
        request.itemLevel = EquipmentItemLevelResolver.Resolve(selectedMap, archetypeDefinition);
        request.accessoryDropChanceMultiplier = GetAtlasMultiplier(AtlasEffectType.AccessoryDropChancePercent);
        request.armorImplicitDropChanceMultiplier = GetAtlasMultiplier(AtlasEffectType.ArmorImplicitArmorDropChancePercent);
        request.evasionImplicitDropChanceMultiplier = GetAtlasMultiplier(AtlasEffectType.ArmorImplicitEvasionDropChancePercent);
        request.barrierImplicitDropChanceMultiplier = GetAtlasMultiplier(AtlasEffectType.ArmorImplicitBarrierDropChancePercent);
        request.accessoriesAlwaysHighestImplicit = HasAtlasEffect(AtlasEffectType.AccessoriesAlwaysHighestImplicit);
        request.forceArmorImplicitPercentArmorPrefix = HasAtlasEffect(AtlasEffectType.ArmorImplicitArmorAlwaysRollPercentArmorPrefix);
        request.forceEvasionImplicitPercentEvasionPrefix = HasAtlasEffect(AtlasEffectType.ArmorImplicitEvasionAlwaysRollPercentEvasionPrefix);
        request.forceBarrierImplicitPercentBarrierPrefix = HasAtlasEffect(AtlasEffectType.ArmorImplicitBarrierAlwaysRollPercentBarrierPrefix);
        request.additionalAffixesForRareItems = GetAtlasCount(AtlasEffectType.RareItemsAdditionalAffixes);
        return request;
    }

    public static void GetAdjustedRarityWeights(
        EquipmentDropSettings settings,
        out float commonWeight,
        out float uncommonWeight,
        out float rareWeight)
    {
        commonWeight = settings != null ? settings.CommonWeight : EquipmentGenerator.DefaultCommonWeight;
        uncommonWeight = settings != null ? settings.UncommonWeight : EquipmentGenerator.DefaultUncommonWeight;
        rareWeight = settings != null ? settings.RareWeight : EquipmentGenerator.DefaultRareWeight;

        float higherRarityMultiplier = GetAtlasMultiplier(AtlasEffectType.EquipmentRarityPercent);
        uncommonWeight *= higherRarityMultiplier;
        rareWeight *= higherRarityMultiplier;
    }

    private static bool HasAtlasEffect(AtlasEffectType effectType) =>
        MetaProgressionService.GetAtlasEffectValue(effectType) > 0f;

    private static float GetAtlasMultiplier(AtlasEffectType effectType) =>
        Mathf.Max(0f, 1f + (MetaProgressionService.GetAtlasEffectValue(effectType) / 100f));

    private static int GetAtlasCount(AtlasEffectType effectType) =>
        Mathf.Max(0, Mathf.RoundToInt(MetaProgressionService.GetAtlasEffectValue(effectType)));
}
