using System.Collections.Generic;
using UnityEngine;

public enum MapAffixTier
{
    Common,
    Uncommon,
    Rare,
}

public enum MapStatType
{
    EnemyQuantity,
    EnemyQuality,
    DropChance,
    EnemyDamage,
    EnemyHealth,
    EnemyMoveSpeed,
    ExperienceWorth,
}

[System.Serializable]
public struct MapModifierValue
{
    public MapStatType statType;
    public float percent;

    public MapModifierValue(MapStatType statType, float percent)
    {
        this.statType = statType;
        this.percent = percent;
    }
}

[System.Serializable]
public class MapAffixDefinition
{
    public string name;
    public MapAffixTier tier;
    public List<MapModifierRange> modifiers = new List<MapModifierRange>();

    public MapAffixDefinition(string name, MapAffixTier tier, params MapModifierRange[] modifiers)
    {
        this.name = name;
        this.tier = tier;
        this.modifiers = new List<MapModifierRange>(modifiers);
    }
}

[System.Serializable]
public struct MapModifierRange
{
    public MapStatType statType;
    public float minPercent;
    public float maxPercent;

    public MapModifierRange(MapStatType statType, float minPercent, float maxPercent)
    {
        this.statType = statType;
        this.minPercent = minPercent;
        this.maxPercent = maxPercent;
    }

    public float Roll()
    {
        return Random.Range(minPercent, maxPercent);
    }
}

[System.Serializable]
public class GeneratedMap
{
    public string baseName;
    public MapAffixDefinition prefix;
    public MapAffixDefinition suffix;
    public List<MapModifierValue> modifiers = new List<MapModifierValue>();

    public string DisplayName
    {
        get
        {
            string prefixName = prefix != null ? prefix.name + " " : string.Empty;
            string suffixName = suffix != null ? " " + suffix.name : string.Empty;
            return prefixName + baseName + suffixName;
        }
    }

    public float GetModifier(MapStatType statType)
    {
        float total = 0f;

        foreach (MapModifierValue modifier in modifiers)
        {
            if (modifier.statType == statType)
            {
                total += modifier.percent;
            }
        }

        return total;
    }
}
