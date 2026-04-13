using UnityEngine;

public static class EquipmentIconResolver
{
    public static Sprite ResolveIcon(EquipmentBaseDefinition baseDefinition)
    {
        if (baseDefinition == null)
        {
            return null;
        }

        if (baseDefinition.Icon != null)
        {
            return baseDefinition.Icon;
        }

        EquipmentIconCatalog iconCatalog = EquipmentCatalogResources.IconCatalog;
        return iconCatalog != null ? iconCatalog.GetDefaultIcon(baseDefinition.SlotType) : null;
    }
}
