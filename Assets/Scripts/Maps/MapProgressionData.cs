public static class MapProgressionData
{
    public static bool IsCompleted(string baseMapId)
    {
        return MetaProgressionService.IsMapCompleted(baseMapId);
    }

    public static bool MarkCompleted(string baseMapId)
    {
        return MetaProgressionService.MarkMapCompleted(baseMapId);
    }

    public static void Reset()
    {
        MetaProgressionService.ResetAllProgress();
    }
}
