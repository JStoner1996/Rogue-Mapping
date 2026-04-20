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

    private void Start()
    {
        InitializeStaging();
    }

    public void ShowWeaponsTab()
    {
        ShowTab(StagingTabController.Tab.Weapons);
    }

    public void ShowMapsTab()
    {
        ShowTab(StagingTabController.Tab.Maps);
    }

    public void ShowEquipmentTab()
    {
        ShowTab(StagingTabController.Tab.Equipment);
    }

    public void StartRun()
    {
        WeaponData selectedWeapon = weaponController?.SelectedWeapon;
        MapInstance selectedMap = mapController?.SelectedMap;
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
        SceneManager.LoadScene(SceneCatalog.Game);
    }

    public void RefreshEquipmentDebugData()
    {
        equipmentController?.RefreshDebugData();
    }

    private void InitializeStaging()
    {
        MetaProgressionService.EnsureLoaded();
        InitializeControllers();
        tabController.RegisterTabButtons(ShowWeaponsTab, ShowMapsTab, ShowEquipmentTab);
        equipmentController.RegisterDropTargets();
        ShowWeaponsTab();
    }

    private void ShowTab(StagingTabController.Tab tab)
    {
        tabController?.SwitchTab(tab, HandleTabChanged);
    }

    private void InitializeControllers()
    {
        weaponController = new WeaponStagingController(weaponGrid, weaponPreviewUI);
        weaponController.Load();

        mapController = new MapStagingController(mapGrid, mapPreviewUI);
        mapController.Load(defaultMapVictoryCondition, defaultMapVictoryTarget);

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
        RunData.SelectedWeapon = weaponController?.SelectedWeapon;
        RunData.SelectedMap = mapController?.SelectedMap;
    }

    private void HandleTabChanged(StagingTabController.Tab tab)
    {
        IStagingTabController activeController = GetActiveTabController();
        activeController?.RefreshGrid();
        activeController?.RefreshPreview();
    }

    private IStagingTabController GetActiveTabController() =>
        (tabController != null ? tabController.CurrentTab : StagingTabController.Tab.Weapons) switch
        {
            StagingTabController.Tab.Weapons => weaponController,
            StagingTabController.Tab.Maps => mapController,
            StagingTabController.Tab.Equipment => equipmentController,
            _ => null,
        };
}
