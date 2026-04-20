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
public enum VictoryConditionType
{
    Time,
    Kills,
}

[System.Serializable]
public enum MapTilesetTheme
{
    Default,
    Shrine,
    Waterways,
    Fields,
    Valley,
    Catacombs,
    Grove,
    Crossroads,
    Sanctum,
    Marsh,
    Ruins,
    Hollow,
    Terrace,
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
public class MapBaseDefinition
{
    public string id;
    public string displayName;
    public int tier;
    public MapTilesetTheme tilesetTheme;
    public string sceneName;
    public Sprite icon;
    public MapWorldThemeDefinition worldTheme;

    public MapBaseDefinition(
        string id,
        string displayName,
        int tier,
        MapTilesetTheme tilesetTheme,
        string sceneName = "",
        Sprite icon = null,
        MapWorldThemeDefinition worldTheme = null)
    {
        this.id = id;
        this.displayName = displayName;
        this.tier = tier;
        this.tilesetTheme = tilesetTheme;
        this.sceneName = sceneName;
        this.icon = icon;
        this.worldTheme = worldTheme;
    }
}

[System.Serializable]
public class MapInstance
{
    public MapBaseDefinition baseMap;
    public MapAffixDefinition prefix;
    public MapAffixDefinition suffix;
    public List<MapModifierValue> modifiers = new List<MapModifierValue>();

    public string BaseMapId => baseMap?.id ?? string.Empty;
    public string BaseMapName => baseMap?.displayName ?? "Unknown Map";
    public int Tier => baseMap?.tier ?? 0;
    public MapTilesetTheme TilesetTheme => baseMap?.tilesetTheme ?? MapTilesetTheme.Default;
    public string SceneName => baseMap?.sceneName ?? string.Empty;
    public Sprite Icon => baseMap != null ? MapIconCatalog.ResolveIcon(baseMap.icon) : MapIconCatalog.PlaceholderMapIcon;
    public MapWorldThemeDefinition WorldTheme => baseMap?.worldTheme;
    public VictoryConditionType VictoryConditionType { get; set; }
    public int VictoryTarget { get; set; }

    public MapAffixTier Rarity
    {
        get
        {
            MapAffixTier rarity = prefix?.tier ?? MapAffixTier.Common;
            return suffix != null && suffix.tier > rarity ? suffix.tier : rarity;
        }
    }

    public bool IsBaseMapCompleted => MapProgressionData.IsCompleted(BaseMapId);

    public string DisplayName
    {
        get
        {
            return $"{(prefix != null ? prefix.name + " " : string.Empty)}{BaseMapName}{(suffix != null ? " " + suffix.name : string.Empty)}";
        }
    }

    public float GetModifier(MapStatType statType)
    {
        float total = 0f;
        for (int i = 0; i < modifiers.Count; i++) if (modifiers[i].statType == statType) total += modifiers[i].percent;

        return total;
    }
}
