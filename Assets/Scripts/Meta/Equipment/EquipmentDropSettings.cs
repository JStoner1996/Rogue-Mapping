using System;
using UnityEngine;

// The generation rules used when an enemy drops equipment into the current run.
[Serializable]
public class EquipmentDropSettings
{
    [Header("Tier Range")]
    [SerializeField] private bool useSelectedMapTier = true;
    [SerializeField] private int minTierOffset = -1;
    [SerializeField] private int maxTierOffset = 1;
    [SerializeField, Min(1)] private int fallbackMinTier = 1;
    [SerializeField, Min(1)] private int fallbackMaxTier = 3;
    [SerializeField, Min(1)] private int maximumGeneratedTier = 10;

    [Header("Rarity Weights")]
    [SerializeField, Min(0f)] private float commonWeight = EquipmentGenerator.DefaultCommonWeight;
    [SerializeField, Min(0f)] private float uncommonWeight = EquipmentGenerator.DefaultUncommonWeight;
    [SerializeField, Min(0f)] private float rareWeight = EquipmentGenerator.DefaultRareWeight;

    public float CommonWeight => commonWeight;
    public float UncommonWeight => uncommonWeight;
    public float RareWeight => rareWeight;

    public EquipmentGenerationRequest BuildRequest(MapInstance selectedMap)
    {
        if (useSelectedMapTier && selectedMap != null)
        {
            int baseTier = Mathf.Max(1, selectedMap.Tier);
            int resolvedMinTier = Mathf.Clamp(baseTier + Mathf.Min(minTierOffset, maxTierOffset), 1, maximumGeneratedTier);
            int resolvedMaxTier = Mathf.Clamp(baseTier + Mathf.Max(minTierOffset, maxTierOffset), resolvedMinTier, maximumGeneratedTier);

            return new EquipmentGenerationRequest
            {
                minItemTier = resolvedMinTier,
                maxItemTier = resolvedMaxTier,
            };
        }

        return new EquipmentGenerationRequest
        {
            minItemTier = Mathf.Clamp(Mathf.Min(fallbackMinTier, fallbackMaxTier), 1, maximumGeneratedTier),
            maxItemTier = Mathf.Clamp(Mathf.Max(fallbackMinTier, fallbackMaxTier), 1, maximumGeneratedTier),
        };
    }
}
