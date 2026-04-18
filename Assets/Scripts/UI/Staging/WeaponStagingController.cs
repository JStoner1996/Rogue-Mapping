using UnityEngine;

// Coordinates the weapons tab state, selection, and preview updates.
public class WeaponStagingController : SimpleListStagingController<WeaponData>
{
    private const string DefaultWeaponName = "Area Weapon";

    public WeaponStagingController(InventoryGridUI weaponGrid, ItemDetailsPanelUI weaponPreviewUI)
        : base(weaponGrid, weaponPreviewUI != null ? new System.Action<WeaponData>(weaponPreviewUI.ShowWeapon) : null)
    {
    }

    public WeaponData SelectedWeapon => SelectedItem;

    public void Load()
    {
        LoadItems(Resources.LoadAll<WeaponData>("WeaponData"), weapon => weapon.weaponName == DefaultWeaponName);
    }

    protected override string GetItemId(WeaponData item) => item.weaponName;
    protected override string GetItemLabel(WeaponData item) => item.weaponName;
    protected override Sprite GetItemIcon(WeaponData item) => item.icon;
    protected override void OnSelectionChanged(WeaponData item) => RunData.SelectedWeapon = item;
}
