using UnityEngine;

// Resolves the exact backend item level used to gate affix availability.
public static class EquipmentItemLevelResolver
{
    private const int MapTierLevelStep = 10;
    private const int BaseMapTierLevel = 10;

    public static int Resolve(MapInstance map, EnemyArchetypeDefinition archetypeDefinition)
    {
        int mapTier = Mathf.Max(1, map != null ? map.Tier : 1);
        int baseItemLevel = BaseMapTierLevel + ((mapTier - 1) * MapTierLevelStep);
        int archetypeOffset = archetypeDefinition != null ? archetypeDefinition.ItemLevelOffset : 0;
        return Mathf.Max(1, baseItemLevel + archetypeOffset);
    }
}
