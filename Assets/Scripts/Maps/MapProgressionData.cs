using System.Collections.Generic;

public static class MapProgressionData
{
    private static readonly HashSet<string> CompletedBaseMapIds = new HashSet<string>();

    public static bool IsCompleted(string baseMapId)
    {
        return !string.IsNullOrEmpty(baseMapId) && CompletedBaseMapIds.Contains(baseMapId);
    }

    public static bool MarkCompleted(string baseMapId)
    {
        if (string.IsNullOrEmpty(baseMapId))
        {
            return false;
        }

        return CompletedBaseMapIds.Add(baseMapId);
    }

    public static void Reset()
    {
        CompletedBaseMapIds.Clear();
    }
}
