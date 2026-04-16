using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StagingManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject weaponsPanel;
    [SerializeField] private GameObject mapsPanel;
    [SerializeField] private GameObject equipmentPanel;

    [Header("Grids")]
    [SerializeField] private InventoryGridUI weaponGrid;
    [SerializeField] private InventoryGridUI mapGrid;
    [SerializeField] private InventoryGridUI equipmentGrid;

    [Header("Previews")]
    [SerializeField] private ItemDetailsPanelUI weaponPreviewUI;
    [SerializeField] private ItemDetailsPanelUI mapPreviewUI;
    [SerializeField] private ItemDetailsPanelUI equipmentPreviewUI;
    [SerializeField] private PlayerStatsPanelUI playerStatsPanelUI;

    [Header("Player Loadout Slots")]
    [SerializeField] private List<EquipmentSlotDropTargetUI> equipmentDropTargets = new List<EquipmentSlotDropTargetUI>();

    [Header("Tab Buttons")]
    [SerializeField] private UnityEngine.UI.Button weaponsTabButton;
    [SerializeField] private UnityEngine.UI.Button mapsTabButton;
    [SerializeField] private UnityEngine.UI.Button equipmentTabButton;
    [SerializeField] private Color activeTabColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color inactiveTabColor = new Color(0.17f, 0.17f, 0.17f, 0.1f);

    [Header("Default Map")]
    [SerializeField] private VictoryConditionType defaultMapVictoryCondition = VictoryConditionType.Kills;
    [SerializeField] private int defaultMapVictoryTarget = 10;

    private EquipmentStagingController equipmentController;
    private MapStagingController mapController;
    private StagingTabController tabController;
    private WeaponStagingController weaponController;

    void Start()
    {
        MetaProgressionService.EnsureLoaded();
        InitializeControllers();
        tabController.RegisterTabButtons(ShowWeaponsTab, ShowMapsTab, ShowEquipmentTab);
        equipmentController.RegisterDropTargets();
        SwitchToTab(StagingTabController.Tab.Weapons);
    }

    public void ShowWeaponsTab()
    {
        SwitchToTab(StagingTabController.Tab.Weapons);
    }

    public void ShowMapsTab()
    {
        SwitchToTab(StagingTabController.Tab.Maps);
    }

    public void ShowEquipmentTab()
    {
        SwitchToTab(StagingTabController.Tab.Equipment);
    }

    public void StartRun()
    {
        WeaponData selectedWeapon = weaponController != null ? weaponController.SelectedWeapon : null;
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

    private void SwitchToTab(StagingTabController.Tab tab)
    {
        tabController?.SwitchTab(tab, HandleTabChanged);
    }

    private void InitializeControllers()
    {
        weaponController = new WeaponStagingController(weaponGrid, weaponPreviewUI);
        weaponController.Load();

        mapController = new MapStagingController(mapGrid, mapPreviewUI);
        mapController.LoadStarterMaps(4, defaultMapVictoryCondition, defaultMapVictoryTarget);

        equipmentController = new EquipmentStagingController(
            equipmentGrid,
            equipmentPreviewUI,
            playerStatsPanelUI,
            equipmentDropTargets);
        equipmentController.Load();

        tabController = new StagingTabController(
            weaponsPanel,
            mapsPanel,
            equipmentPanel,
            weaponsTabButton,
            mapsTabButton,
            equipmentTabButton,
            activeTabColor,
            inactiveTabColor);

        InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        RunData.SelectedWeapon = weaponController != null ? weaponController.SelectedWeapon : null;
        RunData.SelectedMap = mapController != null ? mapController.SelectedMap : null;
    }

    private void HandleTabChanged(StagingTabController.Tab tab)
    {
        IStagingTabController activeController = GetActiveTabController();
        activeController?.RefreshGrid();
        activeController?.RefreshPreview();
    }

    private IStagingTabController GetActiveTabController()
    {
        switch (tabController != null ? tabController.CurrentTab : StagingTabController.Tab.Weapons)
        {
            case StagingTabController.Tab.Weapons:
                return weaponController;

            case StagingTabController.Tab.Maps:
                return mapController;

            case StagingTabController.Tab.Equipment:
                return equipmentController;

            default:
                return null;
        }
    }
}
