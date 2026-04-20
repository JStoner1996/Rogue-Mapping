using System.Collections.Generic;
using UnityEngine;

public static class MetaProgressionService
{
    private static MetaProgressionSaveData saveData;
    private static bool isLoaded;

    public static int UnspentAtlasPoints { get { EnsureLoaded(); return saveData.unspentAtlasPoints; } }

    public static void EnsureLoaded()
    {
        if (isLoaded)
        {
            return;
        }

        saveData = MetaPersistence.Load();
        isLoaded = true;
        EnsureEditorStarterEquipmentIfEmpty();
    }

    public static List<MapInstance> GetOwnedMaps()
    {
        EnsureLoaded();
        return ConvertRecords(saveData.ownedMaps, MapGenerator.CreateMapInstanceFromRecord);
    }

    public static List<OwnedEquipmentRecord> GetOwnedEquipment()
    {
        EnsureLoaded();
        return new List<OwnedEquipmentRecord>(saveData.ownedEquipment);
    }

    public static List<EquipmentInstance> GetOwnedEquipmentInstances()
    {
        EnsureLoaded();
        return ConvertRecords(saveData.ownedEquipment, EquipmentRecordConverter.CreateInstance);
    }

    public static EquipmentLoadoutData GetEquipmentLoadout()
    {
        EnsureLoaded();
        return saveData.equipmentLoadout;
    }

    public static List<EquipmentInstance> GetEquippedEquipmentInstances()
    {
        EnsureLoaded();

        List<EquipmentInstance> ownedEquipment = GetOwnedEquipmentInstances();
        Dictionary<string, EquipmentInstance> equipmentById = new Dictionary<string, EquipmentInstance>();

        for (int i = 0; i < ownedEquipment.Count; i++)
        {
            EquipmentInstance instance = ownedEquipment[i];

            if (instance != null && !string.IsNullOrWhiteSpace(instance.InstanceId))
            {
                equipmentById[instance.InstanceId] = instance;
            }
        }

        List<EquipmentInstance> equippedItems = new List<EquipmentInstance>();
        HashSet<string> addedInstanceIds = new HashSet<string>();

        for (int i = 0; i < saveData.equipmentLoadout.equippedItems.Count; i++)
        {
            EquipmentLoadoutSlot loadoutSlot = saveData.equipmentLoadout.equippedItems[i];

            if (loadoutSlot == null || string.IsNullOrWhiteSpace(loadoutSlot.equipmentInstanceId))
            {
                continue;
            }

            if (!addedInstanceIds.Add(loadoutSlot.equipmentInstanceId))
            {
                continue;
            }

            if (equipmentById.TryGetValue(loadoutSlot.equipmentInstanceId, out EquipmentInstance equippedItem))
            {
                equippedItems.Add(equippedItem);
            }
        }

        return equippedItems;
    }

    public static EquipmentStatSummary GetEquippedEquipmentStatSummary()
    {
        return EquipmentStatSummaryCalculator.Calculate(GetEquippedEquipmentInstances());
    }

    public static string GetEquippedItemId(EquipmentSlotType slotType)
    {
        return GetEquippedItemId(slotType.ToString());
    }

    public static string GetEquippedItemId(string loadoutSlotId)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(loadoutSlotId))
        {
            return string.Empty;
        }

        EquipmentLoadoutSlot equippedSlot = saveData.equipmentLoadout.equippedItems.Find(slot => slot.slotId == loadoutSlotId);
        return equippedSlot != null ? equippedSlot.equipmentInstanceId : string.Empty;
    }

    public static void EnsureStarterMaps(
        VictoryConditionType defaultVictoryCondition,
        int defaultVictoryTarget)
    {
        EnsureLoaded();
        EnsureDefaultMap(defaultVictoryCondition, defaultVictoryTarget);

        Save();
    }

    public static void EnsureDefaultMap(
        VictoryConditionType defaultVictoryCondition,
        int defaultVictoryTarget)
    {
        EnsureLoaded();

        OwnedMapRecord existingDefaultMap = FindOwnedMapRecord(MapGenerator.DefaultMapId);

        if (existingDefaultMap == null)
        {
            AddOwnedMap(MapGenerator.CreateDefaultMap(defaultVictoryCondition, defaultVictoryTarget), false);
            return;
        }

        existingDefaultMap.victoryConditionType = defaultVictoryCondition;
        existingDefaultMap.victoryTarget = Mathf.Max(1, defaultVictoryTarget);
    }

    public static MapInstance GetDefaultMap()
    {
        EnsureLoaded();

        OwnedMapRecord defaultMapRecord = FindOwnedMapRecord(MapGenerator.DefaultMapId);

        if (defaultMapRecord == null)
        {
            MapInstance defaultMap = MapGenerator.CreateDefaultMap();
            AddOwnedMap(defaultMap);
            return defaultMap;
        }

        return MapGenerator.CreateMapInstanceFromRecord(defaultMapRecord);
    }

    public static void AddOwnedMap(MapInstance map, bool saveImmediately = true)
    {
        AddOwnedItem(map, MapGenerator.CreateOwnedMapRecord, saveData.ownedMaps, saveImmediately);
    }

    public static void AddOwnedEquipment(EquipmentInstance equipment, bool saveImmediately = true)
    {
        AddOwnedItem(equipment, EquipmentRecordConverter.CreateRecord, saveData.ownedEquipment, saveImmediately);
    }

    public static bool IsMapCompleted(string baseMapId)
    {
        EnsureLoaded();
        return !string.IsNullOrEmpty(baseMapId) && saveData.completedBaseMapIds.Contains(baseMapId);
    }

    public static bool MarkMapCompleted(string baseMapId)
    {
        EnsureLoaded();

        if (string.IsNullOrEmpty(baseMapId) || saveData.completedBaseMapIds.Contains(baseMapId))
        {
            return false;
        }

        saveData.completedBaseMapIds.Add(baseMapId);
        saveData.unspentAtlasPoints++;
        Save();
        return true;
    }

    public static void Save()
    {
        EnsureLoaded();
        MetaPersistence.Save(saveData);
    }

    public static void ResetMapProgression()
    {
        MutateAndSave(() =>
        {
            saveData.completedBaseMapIds.Clear();
            saveData.unspentAtlasPoints = 0;
        });
    }

    public static void ClearMapInventory()
    {
        MutateAndSave(() => saveData.ownedMaps.Clear());
    }

    public static void ClearEquipmentInventory()
    {
        MutateAndSave(() => saveData.ownedEquipment.Clear());
    }

    public static void ClearEquipmentLoadout()
    {
        MutateAndSave(() => saveData.equipmentLoadout = new EquipmentLoadoutData());
    }

    public static void SetEquippedItem(EquipmentSlotType slotType, string equipmentInstanceId, bool saveImmediately = true)
    {
        SetEquippedItem(slotType.ToString(), equipmentInstanceId, saveImmediately);
    }

    public static void SetEquippedItem(string loadoutSlotId, string equipmentInstanceId, bool saveImmediately = true)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(loadoutSlotId))
        {
            return;
        }

        EquipmentLoadoutSlot existingSlot = saveData.equipmentLoadout.equippedItems.Find(slot => slot.slotId == loadoutSlotId);

        if (string.IsNullOrWhiteSpace(equipmentInstanceId))
        {
            saveData.equipmentLoadout.equippedItems.Remove(existingSlot);
        }
        else
        {
            UnequipFromOtherSlots(loadoutSlotId, equipmentInstanceId);

            if (existingSlot == null)
            {
                existingSlot = new EquipmentLoadoutSlot
                {
                    slotId = loadoutSlotId,
                };
                saveData.equipmentLoadout.equippedItems.Add(existingSlot);
            }

            existingSlot.equipmentInstanceId = equipmentInstanceId;
        }

        PersistIfRequested(saveImmediately);
    }

    public static void ResetAllProgress()
    {
        saveData = MetaPersistence.CreateEmptySave();
        isLoaded = true;
        EnsureEditorStarterEquipmentIfEmpty();
        Save();
    }

    private static void EnsureEditorStarterEquipmentIfEmpty()
    {
#if UNITY_EDITOR
        if (saveData == null || saveData.ownedEquipment.Count > 0)
        {
            return;
        }

        EquipmentBaseCatalog baseCatalog = EquipmentCatalogResources.BaseCatalog;
        EquipmentAffixCatalog affixCatalog = EquipmentCatalogResources.AffixCatalog;

        if (baseCatalog == null || affixCatalog == null)
        {
            return;
        }

        foreach (EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentSlotType)))
        {
            EquipmentInstance starterItem = EquipmentGenerator.Generate(
                baseCatalog,
                affixCatalog,
                new EquipmentGenerationRequest
                {
                    minItemTier = 1,
                    maxItemTier = 10,
                    forceSlotType = true,
                    forcedSlotType = slotType,
                },
                33f,
                33f,
                34f);

            if (starterItem != null)
            {
                AddOwnedEquipment(starterItem, false);
            }
        }
#endif
    }

    private static void UnequipFromOtherSlots(string targetLoadoutSlotId, string equipmentInstanceId)
    {
        for (int i = saveData.equipmentLoadout.equippedItems.Count - 1; i >= 0; i--)
        {
            EquipmentLoadoutSlot equippedSlot = saveData.equipmentLoadout.equippedItems[i];

            if (equippedSlot == null)
            {
                continue;
            }

            if (equippedSlot.slotId != targetLoadoutSlotId && equippedSlot.equipmentInstanceId == equipmentInstanceId)
            {
                saveData.equipmentLoadout.equippedItems.RemoveAt(i);
            }
        }
    }

    private static List<TModel> ConvertRecords<TRecord, TModel>(IReadOnlyList<TRecord> records, System.Func<TRecord, TModel> converter)
        where TModel : class
    {
        List<TModel> converted = new List<TModel>(records != null ? records.Count : 0);

        if (records == null || converter == null)
        {
            return converted;
        }

        for (int i = 0; i < records.Count; i++)
        {
            TModel model = converter(records[i]);
            if (model != null)
            {
                converted.Add(model);
            }
        }

        return converted;
    }

    private static void AddOwnedItem<TModel, TRecord>(
        TModel model,
        System.Func<TModel, TRecord> createRecord,
        ICollection<TRecord> destination,
        bool saveImmediately)
        where TModel : class
        where TRecord : class
    {
        EnsureLoaded();

        if (model == null || createRecord == null || destination == null)
        {
            return;
        }

        TRecord record = createRecord(model);

        if (record == null)
        {
            return;
        }

        destination.Add(record);
        PersistIfRequested(saveImmediately);
    }

    private static OwnedMapRecord FindOwnedMapRecord(string baseMapId)
    {
        return saveData.ownedMaps.Find(record => record.baseMapId == baseMapId);
    }

    private static void MutateAndSave(System.Action mutation)
    {
        EnsureLoaded();
        mutation?.Invoke();
        Save();
    }

    private static void PersistIfRequested(bool saveImmediately)
    {
        if (saveImmediately)
        {
            Save();
        }
    }
}
