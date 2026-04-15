using System.Collections.Generic;
using UnityEngine;

public class WeaponStagingController : IStagingTabController
{
    private readonly InventoryGridUI weaponGrid;
    private readonly ItemDetailsPanelUI weaponPreviewUI;

    private List<WeaponData> availableWeapons = new List<WeaponData>();
    private WeaponData selectedWeapon;
    private WeaponData hoveredWeapon;

    public WeaponStagingController(InventoryGridUI weaponGrid, ItemDetailsPanelUI weaponPreviewUI)
    {
        this.weaponGrid = weaponGrid;
        this.weaponPreviewUI = weaponPreviewUI;
    }

    public WeaponData SelectedWeapon => selectedWeapon;

    public void Load()
    {
        availableWeapons = new List<WeaponData>(Resources.LoadAll<WeaponData>("WeaponData"));
        selectedWeapon = availableWeapons.Find(w => w != null && w.weaponName == "Area Weapon");

        if (selectedWeapon == null && availableWeapons.Count > 0)
        {
            selectedWeapon = availableWeapons[0];
        }

        hoveredWeapon = null;
        RunData.SelectedWeapon = selectedWeapon;
    }

    public void RefreshGrid()
    {
        if (weaponGrid == null)
        {
            return;
        }

        List<InventorySlotModel> items = new List<InventorySlotModel>(availableWeapons.Count);
        foreach (WeaponData weapon in availableWeapons)
        {
            if (weapon == null)
            {
                continue;
            }

            items.Add(new InventorySlotModel
            {
                id = weapon.weaponName,
                label = weapon.weaponName,
                icon = weapon.icon,
                isEmpty = false,
                isSelected = weapon == selectedWeapon,
                isHovered = weapon == hoveredWeapon,
                isInteractable = true,
            });
        }

        weaponGrid.SetItems(
            new InventoryGridModel(items, weaponGrid.MaxSlots),
            new InventoryGridInteractions
            {
                OnSlotClicked = OnWeaponSlotClicked,
                OnSlotHoverEnter = OnWeaponSlotHoverEnter,
                OnSlotHoverExit = OnWeaponSlotHoverExit,
            });
    }

    public void RefreshPreview()
    {
        weaponPreviewUI?.ShowWeapon(hoveredWeapon ?? selectedWeapon);
    }

    private void SetSelectedWeapon(WeaponData weapon)
    {
        selectedWeapon = weapon;
        hoveredWeapon = null;
        RunData.SelectedWeapon = weapon;
        weaponPreviewUI?.ShowWeapon(weapon);
        RefreshGrid();
    }

    private void OnWeaponSlotClicked(int index, InventorySlotModel data)
    {
        if (index < 0 || index >= availableWeapons.Count)
        {
            return;
        }

        SetSelectedWeapon(availableWeapons[index]);
    }

    private void OnWeaponSlotHoverEnter(int index, InventorySlotModel data)
    {
        if (index < 0 || index >= availableWeapons.Count)
        {
            return;
        }

        hoveredWeapon = availableWeapons[index];
        weaponPreviewUI?.ShowWeapon(hoveredWeapon);
        RefreshGrid();
    }

    private void OnWeaponSlotHoverExit(int index, InventorySlotModel data)
    {
        hoveredWeapon = null;
        RefreshPreview();
        RefreshGrid();
    }
}
