using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapAffix", menuName = "Maps/Affix Definition")]
public class MapAffixDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string affixName;
    [SerializeField] private MapAffixType affixType;
    [SerializeField] private MapAffixTier affixTier;

    [Header("Restrictions")]
    [SerializeField, Min(0)] private int minMapTier;
    [SerializeField, Min(0)] private int maxMapTier = 10;

    [Header("Modifiers")]
    [SerializeField] private List<MapModifierDefinition> modifiers = new List<MapModifierDefinition>();

    public string AffixName => affixName;
    public MapAffixType AffixType => affixType;
    public MapAffixTier AffixTier => affixTier;
    public int MinMapTier => minMapTier;
    public int MaxMapTier => maxMapTier;
    public IReadOnlyList<MapModifierDefinition> Modifiers => modifiers;

    public bool CanRollFor(int mapTier)
    {
        return mapTier >= minMapTier && mapTier <= maxMapTier;
    }

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(affixName) || maxMapTier < minMapTier || modifiers == null || modifiers.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (!modifiers[i].IsValid())
            {
                return false;
            }
        }

        return true;
    }
}

[CreateAssetMenu(fileName = "MapAffixCatalog", menuName = "Maps/Affix Catalog")]
public class MapAffixCatalog : ScriptableObject
{
    [SerializeField] private List<MapAffixDefinition> affixDefinitions = new List<MapAffixDefinition>();

    public IReadOnlyList<MapAffixDefinition> AffixDefinitions => affixDefinitions;

    public void Initialize(IEnumerable<MapAffixDefinition> definitions)
    {
        affixDefinitions = definitions != null ? new List<MapAffixDefinition>(definitions) : new List<MapAffixDefinition>();
    }

    public List<MapAffixDefinition> GetValidAffixes(MapAffixType affixType, MapAffixTier affixTier, int mapTier)
    {
        List<MapAffixDefinition> validAffixes = new List<MapAffixDefinition>();

        for (int i = 0; i < affixDefinitions.Count; i++)
        {
            MapAffixDefinition definition = affixDefinitions[i];
            if (definition == null || !definition.IsConfigured())
            {
                continue;
            }

            if (definition.AffixType == affixType && definition.AffixTier == affixTier && definition.CanRollFor(mapTier))
            {
                validAffixes.Add(definition);
            }
        }

        return validAffixes;
    }

    public List<MapAffixDefinition> GetValidAffixesUpToTier(MapAffixType affixType, MapAffixTier maxAffixTier, int mapTier)
    {
        List<MapAffixDefinition> validAffixes = new List<MapAffixDefinition>();

        for (int i = 0; i < affixDefinitions.Count; i++)
        {
            MapAffixDefinition definition = affixDefinitions[i];
            if (definition == null || !definition.IsConfigured())
            {
                continue;
            }

            if (definition.AffixType == affixType
                && definition.AffixTier <= maxAffixTier
                && definition.CanRollFor(mapTier))
            {
                validAffixes.Add(definition);
            }
        }

        return validAffixes;
    }

    public List<MapAffixDefinition> GetValidAffixes(MapAffixTier affixTier, int mapTier)
    {
        List<MapAffixDefinition> validAffixes = new List<MapAffixDefinition>();

        for (int i = 0; i < affixDefinitions.Count; i++)
        {
            MapAffixDefinition definition = affixDefinitions[i];
            if (definition == null || !definition.IsConfigured())
            {
                continue;
            }

            if (definition.AffixTier == affixTier && definition.CanRollFor(mapTier))
            {
                validAffixes.Add(definition);
            }
        }

        return validAffixes;
    }

    public MapAffixDefinition FindAffix(string affixName)
    {
        if (string.IsNullOrWhiteSpace(affixName))
        {
            return null;
        }

        for (int i = 0; i < affixDefinitions.Count; i++)
        {
            MapAffixDefinition definition = affixDefinitions[i];
            if (definition != null && definition.AffixName == affixName)
            {
                return definition;
            }
        }

        return null;
    }

    public bool HasConfiguredAffixes()
    {
        for (int i = 0; i < affixDefinitions.Count; i++)
        {
            MapAffixDefinition definition = affixDefinitions[i];
            if (definition != null && definition.IsConfigured())
            {
                return true;
            }
        }

        return false;
    }
}
