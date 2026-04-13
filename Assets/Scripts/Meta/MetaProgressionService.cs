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

    public static EquipmentLoadoutData GetEquipmentLoadout()
    {
        EnsureLoaded();
        return saveData.equipmentLoadout;
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

    public static void ResetAllProgress()
    {
        saveData = MetaPersistence.CreateEmptySave();
        isLoaded = true;
        Save();
    }
}
