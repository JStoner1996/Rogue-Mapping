using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapBase", menuName = "Maps/Base Definition")]
public class MapBaseDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string baseId;
    [SerializeField] private string displayName;
    [SerializeField, Min(0)] private int tier;
    [SerializeField] private MapTilesetTheme tilesetTheme;
    [SerializeField] private string sceneName;
    [SerializeField] private Sprite icon;
    [SerializeField] private MapWorldThemeDefinition worldTheme;

    public string BaseId => baseId;
    public string DisplayName => displayName;
    public int Tier => tier;
    public MapTilesetTheme TilesetTheme => tilesetTheme;
    public string SceneName => sceneName;
    public Sprite Icon => icon;
    public MapWorldThemeDefinition WorldTheme => worldTheme;

    public bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(baseId)
            && !string.IsNullOrWhiteSpace(displayName)
            && tier >= 0;
    }
}

[CreateAssetMenu(fileName = "MapBaseCatalog", menuName = "Maps/Base Catalog")]
public class MapBaseCatalog : ScriptableObject
{
    [SerializeField] private List<MapBaseDefinition> baseDefinitions = new List<MapBaseDefinition>();

    public IReadOnlyList<MapBaseDefinition> BaseDefinitions => baseDefinitions;

    public void Initialize(IEnumerable<MapBaseDefinition> definitions)
    {
        baseDefinitions = definitions != null ? new List<MapBaseDefinition>(definitions) : new List<MapBaseDefinition>();
    }

    public List<MapBaseDefinition> GetValidBases(int mapTier)
    {
        List<MapBaseDefinition> validBases = new List<MapBaseDefinition>();
        AppendValidBases(validBases, mapTier);
        return validBases;
    }

    public bool HasValidBases(int mapTier)
    {
        for (int i = 0; i < baseDefinitions.Count; i++)
        {
            MapBaseDefinition definition = baseDefinitions[i];
            if (definition == null || !definition.IsConfigured())
            {
                continue;
            }

            if (definition.Tier == mapTier)
            {
                return true;
            }
        }

        return false;
    }

    public MapBaseDefinition FindBase(string baseId)
    {
        if (string.IsNullOrWhiteSpace(baseId))
        {
            return null;
        }

        for (int i = 0; i < baseDefinitions.Count; i++)
        {
            MapBaseDefinition definition = baseDefinitions[i];
            if (definition != null && definition.BaseId == baseId)
            {
                return definition;
            }
        }

        return null;
    }

    private void AppendValidBases(List<MapBaseDefinition> destination, int mapTier)
    {
        if (destination == null)
        {
            return;
        }

        for (int i = 0; i < baseDefinitions.Count; i++)
        {
            MapBaseDefinition definition = baseDefinitions[i];
            if (definition != null && definition.IsConfigured() && definition.Tier == mapTier)
            {
                destination.Add(definition);
            }
        }
    }
}

public static class MapCatalogResources
{
    private const string BaseCatalogResourcePath = "Maps/Catalogs/MapBaseCatalog";
    private const string AffixCatalogResourcePath = "Maps/Catalogs/MapAffixCatalog";
    private const string BasesResourcePath = "Maps/Bases";

    private static MapBaseCatalog loadedBaseCatalog;
    private static MapAffixCatalog loadedAffixCatalog;

    public static MapBaseCatalog BaseCatalog
    {
        get
        {
            EnsureLoaded();
            return loadedBaseCatalog;
        }
    }

    public static MapAffixCatalog AffixCatalog
    {
        get
        {
            EnsureLoaded();
            return loadedAffixCatalog;
        }
    }

    private static void EnsureLoaded()
    {
        if (loadedBaseCatalog == null)
        {
            loadedBaseCatalog = Resources.Load<MapBaseCatalog>(BaseCatalogResourcePath);
        }

        if (loadedBaseCatalog == null || loadedBaseCatalog.BaseDefinitions == null || loadedBaseCatalog.BaseDefinitions.Count == 0)
        {
            MapBaseDefinition[] baseDefinitions = Resources.LoadAll<MapBaseDefinition>(BasesResourcePath);
            if (baseDefinitions != null && baseDefinitions.Length > 0)
            {
                loadedBaseCatalog = ScriptableObject.CreateInstance<MapBaseCatalog>();
                loadedBaseCatalog.Initialize(baseDefinitions);
            }
        }

        if (loadedAffixCatalog == null)
        {
            loadedAffixCatalog = Resources.Load<MapAffixCatalog>(AffixCatalogResourcePath);
        }
    }
}
