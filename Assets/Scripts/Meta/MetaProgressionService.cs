using System.Collections.Generic;
using UnityEngine;

public static class MetaProgressionService
{
    private static MetaProgressionSaveData saveData;
    private static bool isLoaded;

    public static int UnspentAtlasPoints
    {
        get
        {
            EnsureLoaded();
            return saveData.unspentAtlasPoints;
        }
    }

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

        List<MapInstance> maps = new List<MapInstance>(saveData.ownedMaps.Count);

        foreach (OwnedMapRecord record in saveData.ownedMaps)
        {
            MapInstance map = MapGenerator.CreateMapInstanceFromRecord(record);

            if (map != null)
            {
                maps.Add(map);
            }
        }

        return maps;
    }

    public static List<OwnedEquipmentRecord> GetOwnedEquipment()
    {
        EnsureLoaded();
        return new List<OwnedEquipmentRecord>(saveData.ownedEquipment);
    }

    public static List<EquipmentInstance> GetOwnedEquipmentInstances()
    {
        EnsureLoaded();
        List<EquipmentInstance> equipment = new List<EquipmentInstance>(saveData.ownedEquipment.Count);

        foreach (OwnedEquipmentRecord record in saveData.ownedEquipment)
        {
            EquipmentInstance instance = EquipmentRecordConverter.CreateInstance(record);

            if (instance != null)
            {
                equipment.Add(instance);
            }
        }

        return equipment;
    }

    public static EquipmentLoadoutData GetEquipmentLoadout()
    {
        EnsureLoaded();
        return saveData.equipmentLoadout;
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
        int desiredCount,
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

        OwnedMapRecord existingDefaultMap = saveData.ownedMaps.Find(record => record.baseMapId == "default_map");

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

        OwnedMapRecord defaultMapRecord = saveData.ownedMaps.Find(record => record.baseMapId == "default_map");

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
        if (map == null)
        {
            return;
        }

        EnsureLoaded();

        OwnedMapRecord record = MapGenerator.CreateOwnedMapRecord(map);

        if (record == null)
        {
            return;
        }

        saveData.ownedMaps.Add(record);

        if (saveImmediately)
        {
            Save();
        }
    }

    public static void AddOwnedEquipment(EquipmentInstance equipment, bool saveImmediately = true)
    {
        if (equipment == null)
        {
            return;
        }

        EnsureLoaded();

        OwnedEquipmentRecord record = EquipmentRecordConverter.CreateRecord(equipment);

        if (record == null)
        {
            return;
        }

        saveData.ownedEquipment.Add(record);

        if (saveImmediately)
        {
            Save();
        }
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
        EnsureLoaded();
        saveData.completedBaseMapIds.Clear();
        saveData.unspentAtlasPoints = 0;
        Save();
    }

    public static void ClearMapInventory()
    {
        EnsureLoaded();
        saveData.ownedMaps.Clear();
        Save();
    }

    public static void ClearEquipmentInventory()
    {
        EnsureLoaded();
        saveData.ownedEquipment.Clear();
        Save();
    }

    public static void ClearEquipmentLoadout()
    {
        EnsureLoaded();
        saveData.equipmentLoadout = new EquipmentLoadoutData();
        Save();
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
            if (existingSlot != null)
            {
                saveData.equipmentLoadout.equippedItems.Remove(existingSlot);
            }
        }
        else if (existingSlot != null)
        {
            existingSlot.equipmentInstanceId = equipmentInstanceId;
        }
        else
        {
            saveData.equipmentLoadout.equippedItems.Add(new EquipmentLoadoutSlot
            {
                slotId = loadoutSlotId,
                equipmentInstanceId = equipmentInstanceId,
            });
        }

        if (saveImmediately)
        {
            Save();
        }
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
            EquipmentInstance starterItem = EquipmentGenerator.GenerateForSlot(baseCatalog, affixCatalog, slotType, 1);

            if (starterItem != null)
            {
                AddOwnedEquipment(starterItem, false);
            }
        }
#endif
    }
}
