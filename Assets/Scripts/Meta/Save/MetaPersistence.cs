using System.IO;
using UnityEngine;

public static class MetaPersistence
{
    private const string SaveFileName = "meta-progression.json";

    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static MetaProgressionSaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            return CreateEmptySave();
        }

        string json = File.ReadAllText(SavePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return CreateEmptySave();
        }

        MetaProgressionSaveData saveData = JsonUtility.FromJson<MetaProgressionSaveData>(json);
        return saveData ?? CreateEmptySave();
    }

    public static void Save(MetaProgressionSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        string directory = Path.GetDirectoryName(SavePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
    }

    public static MetaProgressionSaveData CreateEmptySave()
    {
        return new MetaProgressionSaveData();
    }
}
