using System;
using UnityEngine;

// The generation rules used when an enemy drops equipment into the current run.
[Serializable]
public class EquipmentDropSettings
{
    [Header("Item Tier")]
    [SerializeField, Min(1)] private int fallbackItemTier = 1;
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
        if (selectedMap != null)
        {
            int resolvedTier = Mathf.Clamp(Mathf.Max(1, selectedMap.Tier), 1, maximumGeneratedTier);

            return new EquipmentGenerationRequest
            {
                minItemTier = resolvedTier,
                maxItemTier = resolvedTier,
            };
        }

        int fallbackTier = Mathf.Clamp(fallbackItemTier, 1, maximumGeneratedTier);

        return new EquipmentGenerationRequest
        {
            minItemTier = fallbackTier,
            maxItemTier = fallbackTier,
        };
    }
}
