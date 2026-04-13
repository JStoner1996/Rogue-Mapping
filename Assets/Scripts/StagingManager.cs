using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StagingManager : MonoBehaviour
{
    private enum StagingTab
    {
        Weapons,
        Maps,
        Equipment,
    }

    [Header("UI")]
    [SerializeField] private GameObject weaponsPanel;
    [SerializeField] private GameObject mapsPanel;
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private InventoryGridUI weaponGrid;
    [SerializeField] private InventoryGridUI mapGrid;
    [SerializeField] private InventoryGridUI equipmentGrid;
    [SerializeField] private ItemDetailsPanelUI weaponPreviewUI;
    [SerializeField] private ItemDetailsPanelUI mapPreviewUI;
    [SerializeField] private ItemDetailsPanelUI equipmentPreviewUI;
    [SerializeField] private List<EquipmentSlotDropTargetUI> equipmentDropTargets = new List<EquipmentSlotDropTargetUI>();
    [SerializeField] private Button weaponsTabButton;
    [SerializeField] private Button mapsTabButton;
    [SerializeField] private Button equipmentTabButton;
    [SerializeField] private Color activeTabColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color inactiveTabColor = new Color(0.17f, 0.17f, 0.17f, 0.1f);

    [Header("Default Map")]
    [SerializeField] private VictoryConditionType defaultMapVictoryCondition = VictoryConditionType.Kills;
    [SerializeField] private int defaultMapVictoryTarget = 10;

    private readonly List<Button> tabButtons = new List<Button>();
    private readonly List<MapInstance> availableMaps = new List<MapInstance>();
    private readonly List<EquipmentInstance> availableEquipment = new List<EquipmentInstance>();
    private List<WeaponData> allWeapons = new List<WeaponData>();

    private WeaponData selectedWeapon;
    private MapInstance selectedMap;
    private EquipmentInstance selectedEquipment;
    private WeaponData hoveredWeapon;
    private MapInstance hoveredMap;
    private EquipmentInstance hoveredEquipment;
    private StagingTab currentTab;

    void Start()
    {
        MetaProgressionService.EnsureLoaded();
        LoadWeapons();
        LoadMaps();
        LoadEquipment();
        InitializeDefaults();
        RegisterTabButtons();
        RegisterEquipmentDropTargets();
        SwitchTab(StagingTab.Weapons);
    }

    private void LoadWeapons()
    {
        allWeapons = new List<WeaponData>(Resources.LoadAll<WeaponData>("WeaponData"));
    }

    private void LoadMaps()
    {
        MetaProgressionService.EnsureStarterMaps(4, defaultMapVictoryCondition, defaultMapVictoryTarget);
        availableMaps.Clear();
        availableMaps.AddRange(MetaProgressionService.GetOwnedMaps());
    }

    private void LoadEquipment()
    {
        availableEquipment.Clear();
        availableEquipment.AddRange(MetaProgressionService.GetOwnedEquipmentInstances());
    }

    private void InitializeDefaults()
    {
        selectedWeapon = allWeapons.Find(w => w.weaponName == "Area Weapon");

        if (selectedWeapon == null && allWeapons.Count > 0)
        {
            selectedWeapon = allWeapons[0];
        }

        if (availableMaps.Count > 0)
        {
            selectedMap = availableMaps[0];
        }

        if (availableEquipment.Count > 0)
        {
            selectedEquipment = availableEquipment[0];
        }

        RunData.SelectedWeapon = selectedWeapon;
        RunData.SelectedMap = selectedMap;
    }

    private void RegisterTabButtons()
    {
        tabButtons.Clear();
        AddTabButton(weaponsTabButton, ShowWeaponsTab);
        AddTabButton(mapsTabButton, ShowMapsTab);
        AddTabButton(equipmentTabButton, ShowEquipmentTab);
    }

    private void AddTabButton(Button button, UnityEngine.Events.UnityAction onClick)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(onClick);
        button.onClick.AddListener(onClick);
        tabButtons.Add(button);
    }

    private void RegisterEquipmentDropTargets()
    {
        foreach (EquipmentSlotDropTargetUI dropTarget in equipmentDropTargets)
        {
            if (dropTarget == null)
            {
                continue;
            }

            dropTarget.DropReceived -= HandleEquipmentDropped;
            dropTarget.DropReceived += HandleEquipmentDropped;
        }

        RefreshEquippedSlotVisuals();
    }

    private void SwitchTab(StagingTab tab)
    {
        currentTab = tab;
        RefreshPanelVisibility();
        RefreshButtons();
        RefreshPreview();
        RefreshTabVisuals();
    }

    private void RefreshPanelVisibility()
    {
        if (weaponsPanel != null)
        {
            weaponsPanel.SetActive(currentTab == StagingTab.Weapons);
        }

        if (mapsPanel != null)
        {
            mapsPanel.SetActive(currentTab == StagingTab.Maps);
        }

        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(currentTab == StagingTab.Equipment);
        }
    }

    private void RefreshButtons()
    {
        switch (currentTab)
        {
            case StagingTab.Weapons:
                RefreshWeaponGrid();
                break;

            case StagingTab.Maps:
                RefreshMapGrid();
                break;

            case StagingTab.Equipment:
                RefreshEquipmentGrid();
                break;
        }
    }

    private void RefreshWeaponGrid()
    {
        if (weaponGrid == null)
        {
            return;
        }

        List<InventorySlotViewData> items = new List<InventorySlotViewData>(allWeapons.Count);
        WeaponData focusedWeapon = hoveredWeapon != null ? hoveredWeapon : selectedWeapon;

        foreach (WeaponData weapon in allWeapons)
        {
            if (weapon == null)
            {
                continue;
            }

            items.Add(new InventorySlotViewData
            {
                id = weapon.weaponName,
                label = weapon.weaponName,
                icon = weapon.icon,
                isEmpty = false,
                isSelected = weapon == selectedWeapon,
                isFocused = weapon == focusedWeapon,
                isInteractable = true,
            });
        }

        weaponGrid.SetItems(items, OnWeaponSlotClicked, OnWeaponSlotHoverEnter, OnWeaponSlotHoverExit);
    }

    private void RefreshMapGrid()
    {
        if (mapGrid == null)
        {
            return;
        }

        List<InventorySlotViewData> items = new List<InventorySlotViewData>(availableMaps.Count);
        MapInstance focusedMap = hoveredMap != null ? hoveredMap : selectedMap;

        foreach (MapInstance map in availableMaps)
        {
            if (map == null)
            {
                continue;
            }

            items.Add(new InventorySlotViewData
            {
                id = map.BaseMapId + "|" + map.DisplayName,
                label = map.DisplayName,
                icon = map.Icon,
                isEmpty = false,
                isSelected = map == selectedMap,
                isFocused = map == focusedMap,
                isInteractable = true,
            });
        }

        mapGrid.SetItems(items, OnMapSlotClicked, OnMapSlotHoverEnter, OnMapSlotHoverExit);
    }

    private void RefreshPreview()
    {
        switch (currentTab)
        {
            case StagingTab.Weapons:
                weaponPreviewUI.ShowWeapon(hoveredWeapon != null ? hoveredWeapon : selectedWeapon);
                break;

            case StagingTab.Maps:
                mapPreviewUI.ShowMap(hoveredMap != null ? hoveredMap : selectedMap);
                break;

            case StagingTab.Equipment:
                if (equipmentPreviewUI != null)
                {
                    if (hoveredEquipment != null || selectedEquipment != null)
                    {
                        equipmentPreviewUI.ShowEquipment(hoveredEquipment != null ? hoveredEquipment : selectedEquipment);
                    }
                    else
                    {
                        equipmentPreviewUI.ShowEquipment();
                    }
                }
                break;
        }
    }

    private void RefreshEquipmentGrid()
    {
        if (equipmentGrid == null)
        {
            return;
        }

        List<InventorySlotViewData> items = new List<InventorySlotViewData>(availableEquipment.Count);
        EquipmentInstance focusedEquipment = hoveredEquipment != null ? hoveredEquipment : selectedEquipment;

        foreach (EquipmentInstance equipment in availableEquipment)
        {
            if (equipment == null)
            {
                continue;
            }

            items.Add(new InventorySlotViewData
            {
                id = equipment.InstanceId,
                label = equipment.DisplayName,
                icon = equipment.Icon,
                isEmpty = false,
                isSelected = equipment == selectedEquipment,
                isFocused = equipment == focusedEquipment,
                isInteractable = true,
                dragItemType = DragItemType.Equipment,
                hasEquipmentSlotType = true,
                equipmentSlotType = equipment.SlotType,
            });
        }

        equipmentGrid.SetItems(items, OnEquipmentSlotClicked, OnEquipmentSlotHoverEnter, OnEquipmentSlotHoverExit);
    }

    private void RefreshTabVisuals()
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            bool isActive = i == (int)currentTab;
            Image image = tabButtons[i].GetComponent<Image>();

            if (image != null)
            {
                image.color = isActive ? activeTabColor : inactiveTabColor;
            }
        }
    }

    private void SelectWeapon(WeaponData weapon)
    {
        selectedWeapon = weapon;
        hoveredWeapon = null;
        RunData.SelectedWeapon = weapon;
        weaponPreviewUI.ShowWeapon(weapon);
        RefreshWeaponGrid();
    }

    private void SelectMap(MapInstance map)
    {
        selectedMap = map;
        hoveredMap = null;
        RunData.SelectedMap = map;
        mapPreviewUI.ShowMap(map);
        RefreshMapGrid();
    }

    private void SelectEquipment(EquipmentInstance equipment)
    {
        selectedEquipment = equipment;
        hoveredEquipment = null;
        if (equipmentPreviewUI != null)
        {
            equipmentPreviewUI.ShowEquipment(equipment);
        }

        RefreshEquipmentGrid();
    }

    private void HandleEquipmentDropped(EquipmentSlotDropTargetUI dropTarget, DragItemPayload payload)
    {
        if (dropTarget == null || payload == null || string.IsNullOrWhiteSpace(payload.itemId))
        {
            return;
        }

        EquipmentInstance equipment = availableEquipment.Find(item => item != null && item.InstanceId == payload.itemId);

        if (equipment == null || equipment.SlotType != dropTarget.SlotType)
        {
            return;
        }

        MetaProgressionService.SetEquippedItem(dropTarget.LoadoutSlotId, equipment.InstanceId);
        SelectEquipment(equipment);
        RefreshEquippedSlotVisuals();
    }

    private void RefreshEquippedSlotVisuals()
    {
        for (int i = 0; i < equipmentDropTargets.Count; i++)
        {
            EquipmentSlotDropTargetUI dropTarget = equipmentDropTargets[i];

            if (dropTarget == null)
            {
                continue;
            }

            string equippedItemId = MetaProgressionService.GetEquippedItemId(dropTarget.LoadoutSlotId);
            EquipmentInstance equippedItem = availableEquipment.Find(item => item != null && item.InstanceId == equippedItemId);
            dropTarget.SetDisplayedEquipment(equippedItem);
        }
    }

    private void OnWeaponSlotClicked(int index, InventorySlotViewData data)
    {
        if (index < 0 || index >= allWeapons.Count)
        {
            return;
        }

        SelectWeapon(allWeapons[index]);
    }

    private void OnWeaponSlotHoverEnter(int index, InventorySlotViewData data)
    {
        if (index < 0 || index >= allWeapons.Count)
        {
            return;
        }

        hoveredWeapon = allWeapons[index];
        weaponPreviewUI.ShowWeapon(hoveredWeapon);
        RefreshWeaponGrid();
    }

    private void OnWeaponSlotHoverExit(int index, InventorySlotViewData data)
    {
        hoveredWeapon = null;
        RefreshPreview();
        RefreshWeaponGrid();
    }

    private void OnMapSlotClicked(int index, InventorySlotViewData data)
    {
        if (index < 0 || index >= availableMaps.Count)
        {
            return;
        }

        SelectMap(availableMaps[index]);
    }

    private void OnMapSlotHoverEnter(int index, InventorySlotViewData data)
    {
        if (index < 0 || index >= availableMaps.Count)
        {
            return;
        }

        hoveredMap = availableMaps[index];
        mapPreviewUI.ShowMap(hoveredMap);
        RefreshMapGrid();
    }

    private void OnMapSlotHoverExit(int index, InventorySlotViewData data)
    {
        hoveredMap = null;
        RefreshPreview();
        RefreshMapGrid();
    }

    private void OnEquipmentSlotClicked(int index, InventorySlotViewData data)
    {
        if (index < 0 || index >= availableEquipment.Count)
        {
            return;
        }

        SelectEquipment(availableEquipment[index]);
    }

    private void OnEquipmentSlotHoverEnter(int index, InventorySlotViewData data)
    {
        if (index < 0 || index >= availableEquipment.Count)
        {
            return;
        }

        hoveredEquipment = availableEquipment[index];
        if (equipmentPreviewUI != null)
        {
            equipmentPreviewUI.ShowEquipment(hoveredEquipment);
        }

        RefreshEquipmentGrid();
    }

    private void OnEquipmentSlotHoverExit(int index, InventorySlotViewData data)
    {
        hoveredEquipment = null;
        RefreshPreview();
        RefreshEquipmentGrid();
    }

    public void ShowWeaponsTab()
    {
        SwitchTab(StagingTab.Weapons);
    }

    public void ShowMapsTab()
    {
        SwitchTab(StagingTab.Maps);
    }

    public void ShowEquipmentTab()
    {
        SwitchTab(StagingTab.Equipment);
    }

    public void StartRun()
    {
        if (selectedWeapon == null)
        {
            Debug.LogWarning("No weapon selected!");
            return;
        }

        if (selectedMap == null)
        {
            Debug.LogWarning("No map selected!");
            return;
        }

        RunData.SelectedWeapon = selectedWeapon;
        RunData.SelectedMap = selectedMap;
        SceneManager.LoadScene("Game");
    }
}
