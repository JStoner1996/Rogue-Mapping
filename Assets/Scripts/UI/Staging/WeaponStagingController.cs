using System.Collections.Generic;
using UnityEngine;

// Coordinates the weapons tab state, selection, and preview updates.
public class WeaponStagingController : SimpleListStagingController<WeaponData>
{
    public WeaponStagingController(InventoryGridUI weaponGrid, ItemDetailsPanelUI weaponPreviewUI)
        : base(weaponGrid, weaponPreviewUI != null ? new System.Action<WeaponData>(weaponPreviewUI.ShowWeapon) : null)
    {
    }

    public WeaponData SelectedWeapon => SelectedItem;

    public void Load()
    {
        List<WeaponData> availableWeapons = new List<WeaponData>(Resources.LoadAll<WeaponData>("WeaponData"));
        SetItems(availableWeapons);
        SelectedItem = availableWeapons.Find(w => w != null && w.weaponName == "Area Weapon");

        if (SelectedItem == null && availableWeapons.Count > 0)
        {
            SelectedItem = availableWeapons[0];
        }

        ClearHoveredItem();
        RunData.SelectedWeapon = SelectedItem;
    }

    protected override string GetItemId(WeaponData item) => item.weaponName;
    protected override string GetItemLabel(WeaponData item) => item.weaponName;
    protected override Sprite GetItemIcon(WeaponData item) => item.icon;
    protected override void OnSelectionChanged(WeaponData item) => RunData.SelectedWeapon = item;
}
