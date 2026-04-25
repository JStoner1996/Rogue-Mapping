using System;
using System.Collections.Generic;
using UnityEngine;

public enum MapAffixType
{
    Prefix,
    Suffix,
}

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
    MapDropChance,
    MapRarity,
    EquipmentDropChance,
    EquipmentRarity,
    EnemyDamage,
    EnemyHealth,
    EnemyMoveSpeed,
    ExperienceWorth,
    EliteChance,
    TankChance,
    SkirmisherChance,
    EliteDamage,
    EliteHealth,
    TankDamage,
    TankHealth,
    SkirmisherDamage,
    SkirmisherHealth,
}

[Serializable]
public enum VictoryConditionType
{
    Time,
    Kills,
}

[Serializable]
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

[Serializable]
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

[Serializable]
public struct MapModifierDefinition
{
    public MapStatType statType;
    public float minPercent;
    public float maxPercent;

    public MapModifierDefinition(MapStatType statType, float minPercent, float maxPercent)
    {
        this.statType = statType;
        this.minPercent = minPercent;
        this.maxPercent = maxPercent;
    }

    public float Roll()
    {
        if (Mathf.Approximately(minPercent, maxPercent))
        {
            return minPercent;
        }

        return UnityEngine.Random.Range(Mathf.Min(minPercent, maxPercent), Mathf.Max(minPercent, maxPercent));
    }

    public bool IsValid()
    {
        return maxPercent >= minPercent;
    }
}

[Serializable]
public class MapRolledAffix
{
    [SerializeField] private MapAffixDefinition affixDefinition;
    [SerializeField] private List<MapModifierValue> modifierRolls = new List<MapModifierValue>();

    public MapAffixDefinition AffixDefinition => affixDefinition;
    public string AffixName => affixDefinition != null ? affixDefinition.AffixName : string.Empty;
    public MapAffixType AffixType => affixDefinition != null ? affixDefinition.AffixType : default;
    public MapAffixTier AffixTier => affixDefinition != null ? affixDefinition.AffixTier : MapAffixTier.Common;
    public IReadOnlyList<MapModifierValue> ModifierRolls => modifierRolls;

    public MapRolledAffix(MapAffixDefinition affixDefinition, List<MapModifierValue> modifierRolls)
    {
        this.affixDefinition = affixDefinition;
        this.modifierRolls = modifierRolls ?? new List<MapModifierValue>();
    }
}

[Serializable]
public class MapInstance
{
    [SerializeField] private string instanceId;
    [SerializeField] private MapBaseDefinition baseDefinition;
    [SerializeField] private MapAffixTier rarity;
    [SerializeField] private List<MapRolledAffix> prefixAffixes = new List<MapRolledAffix>();
    [SerializeField] private List<MapRolledAffix> suffixAffixes = new List<MapRolledAffix>();
    [SerializeField] private List<MapRolledAffix> additionalAffixes = new List<MapRolledAffix>();
    [SerializeField] private string displayPrefixAffixName;
    [SerializeField] private string displaySuffixAffixName;

    public string InstanceId => instanceId;
    public MapBaseDefinition BaseDefinition => baseDefinition;
    public MapAffixTier Rarity => rarity;
    public IReadOnlyList<MapRolledAffix> PrefixAffixes => prefixAffixes;
    public IReadOnlyList<MapRolledAffix> SuffixAffixes => suffixAffixes;
    public IReadOnlyList<MapRolledAffix> AdditionalAffixes => additionalAffixes;
    public int PrefixAffixCount => prefixAffixes != null ? prefixAffixes.Count : 0;
    public int SuffixAffixCount => suffixAffixes != null ? suffixAffixes.Count : 0;
    public string DisplayPrefixAffixName => displayPrefixAffixName;
    public string DisplaySuffixAffixName => displaySuffixAffixName;

    public string BaseMapId => baseDefinition != null ? baseDefinition.BaseId : string.Empty;
    public string BaseMapName => baseDefinition != null ? baseDefinition.DisplayName : "Unknown Map";
    public int Tier => baseDefinition != null ? baseDefinition.Tier : 0;
    public MapTilesetTheme TilesetTheme => baseDefinition != null ? baseDefinition.TilesetTheme : MapTilesetTheme.Default;
    public string SceneName => baseDefinition != null ? baseDefinition.SceneName : string.Empty;
    public Sprite Icon => baseDefinition != null ? MapIconCatalog.ResolveIcon(baseDefinition.Icon) : MapIconCatalog.PlaceholderMapIcon;
    public MapWorldThemeDefinition WorldTheme => baseDefinition != null ? baseDefinition.WorldTheme : null;
    public int AdditionalAffixCount => additionalAffixes != null ? additionalAffixes.Count : 0;

    public VictoryConditionType VictoryConditionType { get; set; }
    public int VictoryTarget { get; set; }

    public bool IsBaseMapCompleted => MapProgressionData.IsCompleted(BaseMapId);

    public string DisplayName
    {
        get
        {
            string prefix = string.IsNullOrWhiteSpace(displayPrefixAffixName) ? string.Empty : displayPrefixAffixName + " ";
            string suffix = string.IsNullOrWhiteSpace(displaySuffixAffixName) ? string.Empty : " " + displaySuffixAffixName;
            return $"{prefix}{BaseMapName}{suffix}".Trim();
        }
    }

    public MapInstance(
        string instanceId,
        MapBaseDefinition baseDefinition,
        MapAffixTier rarity,
        List<MapRolledAffix> prefixAffixes,
        List<MapRolledAffix> suffixAffixes,
        List<MapRolledAffix> additionalAffixes,
        string displayPrefixAffixName,
        string displaySuffixAffixName)
    {
        this.instanceId = string.IsNullOrWhiteSpace(instanceId) ? Guid.NewGuid().ToString("N") : instanceId;
        this.baseDefinition = baseDefinition;
        this.rarity = rarity;
        this.prefixAffixes = prefixAffixes ?? new List<MapRolledAffix>();
        this.suffixAffixes = suffixAffixes ?? new List<MapRolledAffix>();
        this.additionalAffixes = additionalAffixes ?? new List<MapRolledAffix>();
        this.displayPrefixAffixName = displayPrefixAffixName ?? string.Empty;
        this.displaySuffixAffixName = displaySuffixAffixName ?? string.Empty;
    }

    public float GetModifier(MapStatType statType)
    {
        float total = 0f;
        IReadOnlyList<MapModifierValue> modifierRolls = GetModifierRolls();

        for (int i = 0; i < modifierRolls.Count; i++)
        {
            if (modifierRolls[i].statType == statType)
            {
                total += modifierRolls[i].percent;
            }
        }

        return total;
    }

    public IReadOnlyList<MapRolledAffix> GetAllAffixes()
    {
        List<MapRolledAffix> affixes = new List<MapRolledAffix>();
        AppendAffixes(prefixAffixes, affixes);
        AppendAffixes(suffixAffixes, affixes);
        AppendAffixes(additionalAffixes, affixes);
        return affixes;
    }

    public IReadOnlyList<MapModifierValue> GetModifierRolls()
    {
        List<MapModifierValue> rolls = new List<MapModifierValue>();
        AppendAffixRolls(prefixAffixes, rolls);
        AppendAffixRolls(suffixAffixes, rolls);
        AppendAffixRolls(additionalAffixes, rolls);
        return rolls;
    }

    private static void AppendAffixRolls(IReadOnlyList<MapRolledAffix> affixes, List<MapModifierValue> output)
    {
        if (affixes == null || output == null)
        {
            return;
        }

        for (int i = 0; i < affixes.Count; i++)
        {
            AppendAffixRolls(affixes[i], output);
        }
    }

    private static void AppendAffixRolls(MapRolledAffix affix, List<MapModifierValue> output)
    {
        if (affix?.ModifierRolls == null || output == null)
        {
            return;
        }

        for (int i = 0; i < affix.ModifierRolls.Count; i++)
        {
            output.Add(affix.ModifierRolls[i]);
        }
    }

    private static void AppendAffixes(IReadOnlyList<MapRolledAffix> source, List<MapRolledAffix> destination)
    {
        if (source == null || destination == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            if (source[i] != null)
            {
                destination.Add(source[i]);
            }
        }
    }
}
