using System.Collections.Generic;
using UnityEngine;

public static class EnemyLootDropper
{
    public static void DropAll(
        Vector3 position,
        int experienceWorth,
        float dropChanceMultiplier,
        EnemyArchetypeDefinition archetypeDefinition,
        IReadOnlyList<LootItem> powerUpLootTable,
        IReadOnlyList<MetaLootItem> metaLootTable)
    {
        DropExperience(position, experienceWorth);
        ProcessDrops(
            powerUpLootTable,
            lootItem => lootItem.GetAdjustedDropChance(dropChanceMultiplier),
            lootItem => SpawnPowerUp(position, lootItem));
        ProcessDrops(
            metaLootTable,
            lootItem => lootItem.GetAdjustedDropChance(GetMetaDropChanceMultiplier(lootItem, dropChanceMultiplier)),
            lootItem => SpawnMetaLoot(position, lootItem, archetypeDefinition));
    }

    private static void DropExperience(Vector3 position, int experienceWorth)
    {
        ExpCrystal xp = PickupPools.Instance.GetXP();
        xp.transform.position = position;
        xp.Init(experienceWorth);
    }

    private static void SpawnPowerUp(Vector3 position, LootItem lootItem)
    {
        if (lootItem == null)
        {
            return;
        }

        switch (lootItem.type)
        {
            case PowerUpLootType.Health:
                HealthPickup health = PickupPools.Instance.GetHealth();
                health.transform.position = position;
                break;

            case PowerUpLootType.Magnet:
                Magnet magnet = PickupPools.Instance.GetMagnet();
                magnet.transform.position = position;
                break;

            case PowerUpLootType.Bomb:
                Bomb bomb = PickupPools.Instance.GetBomb();
                bomb.transform.position = position;
                break;
        }
    }

    private static void SpawnMetaLoot(Vector3 position, MetaLootItem lootItem, EnemyArchetypeDefinition archetypeDefinition)
    {
        if (lootItem == null)
        {
            return;
        }

        if (lootItem.type == MetaLootType.Map)
        {
            SpawnMapLoot(position, lootItem);
            return;
        }

        SpawnEquipmentLoot(position, lootItem, archetypeDefinition);
    }

    private static void SpawnMapLoot(Vector3 position, MetaLootItem lootItem)
    {
        MapInstance droppedMap = MapGenerator.CreateDroppedMap(
            RunData.GetSelectedMapOrDefault().Tier,
            lootItem.mapDropSettings);

        if (droppedMap == null)
        {
            return;
        }

        MapPickup mapPickup = PickupPools.Instance.GetMapPickup();
        mapPickup.transform.position = position;
        mapPickup.Initialize(droppedMap);
    }

    private static void SpawnEquipmentLoot(Vector3 position, MetaLootItem lootItem, EnemyArchetypeDefinition archetypeDefinition)
    {
        EquipmentBaseCatalog baseCatalog = EquipmentCatalogResources.BaseCatalog;
        EquipmentAffixCatalog affixCatalog = EquipmentCatalogResources.AffixCatalog;
        MapInstance selectedMap = RunData.GetSelectedMapOrDefault();

        if (baseCatalog == null || affixCatalog == null)
        {
            return;
        }

        EquipmentGenerationRequest request = EquipmentAtlasGenerationRequestBuilder.Build(
            lootItem.equipmentDropSettings,
            selectedMap,
            archetypeDefinition);
        EquipmentAtlasGenerationRequestBuilder.GetAdjustedRarityWeights(
            lootItem.equipmentDropSettings,
            out float commonWeight,
            out float uncommonWeight,
            out float rareWeight);

        EquipmentInstance droppedEquipment = EquipmentGenerator.Generate(
            baseCatalog,
            affixCatalog,
            request,
            commonWeight,
            uncommonWeight,
            rareWeight);

        if (droppedEquipment == null)
        {
            return;
        }

        EquipmentPickup equipmentPickup = PickupPools.Instance.GetEquipmentPickup();
        equipmentPickup.transform.position = position;
        equipmentPickup.Initialize(droppedEquipment);
    }

    private static float GetMetaDropChanceMultiplier(MetaLootItem lootItem, float baseMultiplier)
    {
        EquipmentStatSummary summary = MetaProgressionService.GetEquippedEquipmentStatSummary();
        EquipmentStatType? bonusStatType = GetMetaDropBonusStat(lootItem);
        EquipmentStatSummaryEntry entry = bonusStatType.HasValue ? summary?.GetEntry(bonusStatType.Value) : null;
        float equipmentIncrease = entry != null && entry.HasPercentValue ? entry.percentValue : 0f;
        float atlasIncrease = GetMetaDropAtlasIncrease(lootItem);
        return baseMultiplier * Mathf.Max(0f, 1f + equipmentIncrease + atlasIncrease);
    }

    private static EquipmentStatType? GetMetaDropBonusStat(MetaLootItem lootItem) =>
        lootItem?.type switch
        {
            MetaLootType.Map => EquipmentStatType.MapDropChance,
            MetaLootType.Equipment => EquipmentStatType.EquipmentDropChance,
            _ => null
        };

    private static float GetMetaDropAtlasIncrease(MetaLootItem lootItem)
    {
        if (lootItem == null)
        {
            return 0f;
        }

        AtlasEffectType effectType = lootItem.type == MetaLootType.Map
            ? AtlasEffectType.MapDropChancePercent
            : AtlasEffectType.ItemDropChancePercent;

        return MetaProgressionService.GetAtlasEffectValue(effectType) / 100f;
    }

    private static void ProcessDrops<TLoot>(
        IReadOnlyList<TLoot> lootTable,
        System.Func<TLoot, float> getDropChance,
        System.Action<TLoot> spawnLoot)
        where TLoot : class
    {
        if (lootTable == null || getDropChance == null || spawnLoot == null)
        {
            return;
        }

        for (int i = 0; i < lootTable.Count; i++)
        {
            TLoot lootItem = lootTable[i];

            if (lootItem != null && Random.Range(0f, 100f) <= getDropChance(lootItem))
            {
                spawnLoot(lootItem);
            }
        }
    }
}
