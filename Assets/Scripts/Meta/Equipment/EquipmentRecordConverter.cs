using System.Collections.Generic;
using UnityEngine;

public static class EquipmentRecordConverter
{
    public static OwnedEquipmentRecord CreateRecord(EquipmentInstance equipment)
    {
        if (equipment == null || equipment.BaseDefinition == null)
        {
            return null;
        }

        return new OwnedEquipmentRecord
        {
            instanceId = equipment.InstanceId,
            rarity = equipment.Rarity,
            itemTier = equipment.ItemTier,
            baseName = equipment.BaseName,
            prefixAffixName = equipment.PrefixAffix != null ? equipment.PrefixAffix.AffixName : string.Empty,
            suffixAffixName = equipment.SuffixAffix != null ? equipment.SuffixAffix.AffixName : string.Empty,
            slotId = equipment.SlotType.ToString(),
            implicitRolls = new List<EquipmentModifierRoll>(equipment.ImplicitRolls),
            prefixRolls = new List<EquipmentModifierRoll>(equipment.PrefixRolls),
            suffixRolls = new List<EquipmentModifierRoll>(equipment.SuffixRolls),
        };
    }

    public static EquipmentInstance CreateInstance(OwnedEquipmentRecord record)
    {
        if (record == null)
        {
            return null;
        }

        EquipmentBaseCatalog baseCatalog = EquipmentCatalogResources.BaseCatalog;
        EquipmentAffixCatalog affixCatalog = EquipmentCatalogResources.AffixCatalog;

        if (baseCatalog == null || affixCatalog == null)
        {
            Debug.LogError("Equipment catalogs are required to rebuild saved equipment instances.");
            return null;
        }

        EquipmentBaseDefinition baseDefinition = FindBaseDefinition(baseCatalog, record.baseName);

        if (baseDefinition == null)
        {
            Debug.LogWarning($"Unable to rebuild equipment. Unknown base item: {record.baseName}");
            return null;
        }

        EquipmentAffixDefinition prefixAffix = FindAffixDefinition(affixCatalog, record.prefixAffixName, EquipmentAffixType.Prefix);
        EquipmentAffixDefinition suffixAffix = FindAffixDefinition(affixCatalog, record.suffixAffixName, EquipmentAffixType.Suffix);

        return new EquipmentInstance(
            record.instanceId,
            record.rarity,
            Mathf.Max(1, record.itemTier),
            baseDefinition,
            prefixAffix,
            suffixAffix,
            new List<EquipmentModifierRoll>(record.implicitRolls ?? new List<EquipmentModifierRoll>()),
            new List<EquipmentModifierRoll>(record.prefixRolls ?? new List<EquipmentModifierRoll>()),
            new List<EquipmentModifierRoll>(record.suffixRolls ?? new List<EquipmentModifierRoll>()));
    }

    private static EquipmentBaseDefinition FindBaseDefinition(EquipmentBaseCatalog catalog, string baseName)
    {
        foreach (EquipmentBaseDefinition baseDefinition in catalog.BaseDefinitions)
        {
            if (baseDefinition != null && baseDefinition.BaseName == baseName)
            {
                return baseDefinition;
            }
        }

        return null;
    }

    private static EquipmentAffixDefinition FindAffixDefinition(
        EquipmentAffixCatalog catalog,
        string affixName,
        EquipmentAffixType affixType)
    {
        if (string.IsNullOrWhiteSpace(affixName))
        {
            return null;
        }

        foreach (EquipmentAffixDefinition affixDefinition in catalog.AffixDefinitions)
        {
            if (affixDefinition != null
                && affixDefinition.AffixType == affixType
                && affixDefinition.AffixName == affixName)
            {
                return affixDefinition;
            }
        }

        return null;
    }
}
