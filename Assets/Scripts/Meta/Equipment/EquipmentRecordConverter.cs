using System.Collections.Generic;
using UnityEngine;

public static class EquipmentRecordConverter
{
    private static readonly IReadOnlyList<EquipmentModifierRoll> EmptyRolls = System.Array.Empty<EquipmentModifierRoll>();

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
            itemLevel = equipment.ItemLevel,
            baseName = equipment.BaseName,
            prefixAffixes = CreateAffixRecords(equipment.PrefixAffixes),
            suffixAffixes = CreateAffixRecords(equipment.SuffixAffixes),
            prefixAffixName = equipment.PrefixAffix != null ? equipment.PrefixAffix.AffixName : string.Empty,
            suffixAffixName = equipment.SuffixAffix != null ? equipment.SuffixAffix.AffixName : string.Empty,
            slotId = equipment.SlotType.ToString(),
            implicitRolls = CopyRolls(equipment.ImplicitRolls),
            prefixRolls = CopyRolls(equipment.PrefixRolls),
            suffixRolls = CopyRolls(equipment.SuffixRolls),
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

        return new EquipmentInstance(
            record.instanceId,
            record.rarity,
            Mathf.Max(1, record.itemTier),
            Mathf.Max(1, record.itemLevel > 0 ? record.itemLevel : record.itemTier),
            baseDefinition,
            CopyRolls(record.implicitRolls),
            CreateRolledAffixes(record.prefixAffixes, record.prefixAffixName, record.prefixRolls, EquipmentAffixType.Prefix, affixCatalog),
            CreateRolledAffixes(record.suffixAffixes, record.suffixAffixName, record.suffixRolls, EquipmentAffixType.Suffix, affixCatalog));
    }

    private static List<OwnedEquipmentAffixRecord> CreateAffixRecords(IReadOnlyList<EquipmentRolledAffix> affixes)
    {
        List<OwnedEquipmentAffixRecord> records = new List<OwnedEquipmentAffixRecord>();

        if (affixes == null)
        {
            return records;
        }

        for (int i = 0; i < affixes.Count; i++)
        {
            EquipmentRolledAffix affix = affixes[i];
            if (affix?.AffixDefinition == null)
            {
                continue;
            }

            records.Add(new OwnedEquipmentAffixRecord
            {
                affixName = affix.AffixName,
                affixType = affix.AffixType,
                modifierRolls = CopyRolls(affix.ModifierRolls)
            });
        }

        return records;
    }

    private static List<EquipmentRolledAffix> CreateRolledAffixes(
        List<OwnedEquipmentAffixRecord> records,
        string legacyAffixName,
        List<EquipmentModifierRoll> legacyRolls,
        EquipmentAffixType fallbackType,
        EquipmentAffixCatalog affixCatalog)
    {
        List<EquipmentRolledAffix> rolledAffixes = new List<EquipmentRolledAffix>();

        if (records != null && records.Count > 0)
        {
            for (int i = 0; i < records.Count; i++)
            {
                OwnedEquipmentAffixRecord record = records[i];
                EquipmentAffixDefinition definition = FindAffixDefinition(affixCatalog, record.affixName, record.affixType);
                if (definition == null)
                {
                    continue;
                }

                rolledAffixes.Add(new EquipmentRolledAffix(
                    definition,
                    CopyRolls(record.modifierRolls)));
            }

            return rolledAffixes;
        }

        EquipmentAffixDefinition legacyDefinition = FindAffixDefinition(affixCatalog, legacyAffixName, fallbackType);
        if (legacyDefinition != null)
        {
            rolledAffixes.Add(new EquipmentRolledAffix(
                legacyDefinition,
                CopyRolls(legacyRolls)));
        }

        return rolledAffixes;
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

    private static List<EquipmentModifierRoll> CopyRolls(IReadOnlyList<EquipmentModifierRoll> rolls) =>
        new(rolls ?? EmptyRolls);
}
