public static class RunData
{
    public static WeaponData SelectedWeapon;
    public static MapInstance SelectedMap;

    public static MapInstance GetSelectedMapOrDefault()
    {
        SelectedMap ??= MetaProgressionService.GetDefaultMap();
        return SelectedMap;
    }
}
