public static class RunData
{
    public static WeaponData SelectedWeapon { get; set; }
    public static MapInstance SelectedMap { get; set; }

    public static void ClearSelections()
    {
        SelectedWeapon = null;
        SelectedMap = null;
    }

    // Falls back to a starter/default map so scene boot logic always has a playable destination.
    public static MapInstance GetSelectedMapOrDefault()
    {
        SelectedMap ??= MetaProgressionService.GetDefaultMap();
        return SelectedMap;
    }
}
