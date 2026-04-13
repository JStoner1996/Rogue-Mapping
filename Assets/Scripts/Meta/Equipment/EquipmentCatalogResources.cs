using UnityEngine;

public static class EquipmentCatalogResources
{
    private const string BaseCatalogResourcePath = "Equipment/Catalogs/EquipmentBaseCatalog";
    private const string AffixCatalogResourcePath = "Equipment/Catalogs/EquipmentAffixCatalog";
    private const string IconCatalogResourcePath = "Equipment/Catalogs/EquipmentIconCatalog";

    private static EquipmentBaseCatalog loadedBaseCatalog;
    private static EquipmentAffixCatalog loadedAffixCatalog;
    private static EquipmentIconCatalog loadedIconCatalog;

    public static EquipmentBaseCatalog BaseCatalog
    {
        get
        {
            EnsureLoaded();
            return loadedBaseCatalog;
        }
    }

    public static EquipmentAffixCatalog AffixCatalog
    {
        get
        {
            EnsureLoaded();
            return loadedAffixCatalog;
        }
    }

    public static EquipmentIconCatalog IconCatalog
    {
        get
        {
            EnsureLoaded();
            return loadedIconCatalog;
        }
    }

    private static void EnsureLoaded()
    {
        if (loadedBaseCatalog == null)
        {
            loadedBaseCatalog = Resources.Load<EquipmentBaseCatalog>(BaseCatalogResourcePath);
        }

        if (loadedAffixCatalog == null)
        {
            loadedAffixCatalog = Resources.Load<EquipmentAffixCatalog>(AffixCatalogResourcePath);
        }

        if (loadedIconCatalog == null)
        {
            loadedIconCatalog = Resources.Load<EquipmentIconCatalog>(IconCatalogResourcePath);
        }
    }
}
