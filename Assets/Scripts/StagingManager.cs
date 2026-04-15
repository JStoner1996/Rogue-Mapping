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
    [SerializeField] private EquipmentInventoryDropTargetUI equipmentInventoryDropTarget;
    [SerializeField] private ItemDetailsPanelUI weaponPreviewUI;
    [SerializeField] private ItemDetailsPanelUI mapPreviewUI;
    [SerializeField] private ItemDetailsPanelUI equipmentPreviewUI;
    [SerializeField] private PlayerStatsPanelUI playerStatsPanelUI;
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
    private List<WeaponData> allWeapons = new List<WeaponData>();
    private EquipmentStagingController equipmentController;
    private MapStagingController mapController;

    private WeaponData selectedWeapon;
    private WeaponData hoveredWeapon;
    private StagingTab currentTab;

    void Start()
    {
        MetaProgressionService.EnsureLoaded();
        LoadWeapons();
        mapController = new MapStagingController(mapGrid, mapPreviewUI);
        mapController.LoadStarterMaps(4, defaultMapVictoryCondition, defaultMapVictoryTarget);
        equipmentController = new EquipmentStagingController(
            equipmentGrid,
            equipmentInventoryDropTarget,
            equipmentPreviewUI,
            playerStatsPanelUI,
            equipmentDropTargets);
        equipmentController.Load();
        InitializeDefaults();
        RegisterTabButtons();
        equipmentController.RegisterDropTargets();
        SwitchTab(StagingTab.Weapons);
    }

    private void LoadWeapons()
    {
        allWeapons = new List<WeaponData>(Resources.LoadAll<WeaponData>("WeaponData"));
    }

    private void InitializeDefaults()
    {
        selectedWeapon = allWeapons.Find(w => w.weaponName == "Area Weapon");

        if (selectedWeapon == null && allWeapons.Count > 0)
        {
            selectedWeapon = allWeapons[0];
        }

        RunData.SelectedWeapon = selectedWeapon;
        RunData.SelectedMap = mapController != null ? mapController.SelectedMap : null;
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
                mapController?.RefreshGrid();
                break;

            case StagingTab.Equipment:
                equipmentController?.RefreshGrid();
                break;
        }
    }

    private void RefreshWeaponGrid()
    {
        if (weaponGrid == null)
        {
            return;
        }

        List<InventorySlotModel> items = new List<InventorySlotModel>(allWeapons.Count);
        foreach (WeaponData weapon in allWeapons)
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

    private void RefreshPreview()
    {
        switch (currentTab)
        {
            case StagingTab.Weapons:
                weaponPreviewUI.ShowWeapon(hoveredWeapon != null ? hoveredWeapon : selectedWeapon);
                break;

            case StagingTab.Maps:
                mapController?.RefreshPreview();
                break;

            case StagingTab.Equipment:
                equipmentController?.RefreshPreview();
                break;
        }
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

    private void OnWeaponSlotClicked(int index, InventorySlotModel data)
    {
        if (index < 0 || index >= allWeapons.Count)
        {
            return;
        }

        SelectWeapon(allWeapons[index]);
    }

    private void OnWeaponSlotHoverEnter(int index, InventorySlotModel data)
    {
        if (index < 0 || index >= allWeapons.Count)
        {
            return;
        }

        hoveredWeapon = allWeapons[index];
        weaponPreviewUI.ShowWeapon(hoveredWeapon);
        RefreshWeaponGrid();
    }

    private void OnWeaponSlotHoverExit(int index, InventorySlotModel data)
    {
        hoveredWeapon = null;
        RefreshPreview();
        RefreshWeaponGrid();
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

        MapInstance selectedMap = mapController != null ? mapController.SelectedMap : null;
        if (selectedMap == null)
        {
            Debug.LogWarning("No map selected!");
            return;
        }

        RunData.SelectedWeapon = selectedWeapon;
        RunData.SelectedMap = selectedMap;
        SceneManager.LoadScene("Game");
    }

    public void RefreshEquipmentDebugData()
    {
        equipmentController?.RefreshDebugData();
    }
}
