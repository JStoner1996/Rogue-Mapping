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
    [SerializeField] private StagingPreviewUI weaponPreviewUI;
    [SerializeField] private StagingPreviewUI mapPreviewUI;
    // [SerializeField] private StagingPreviewUI equipmentPreviewUI;
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
    private List<WeaponData> allWeapons = new List<WeaponData>();

    private WeaponData selectedWeapon;
    private MapInstance selectedMap;
    private StagingTab currentTab;

    void Start()
    {
        MetaProgressionService.EnsureLoaded();
        LoadWeapons();
        LoadMaps();
        InitializeDefaults();
        RegisterTabButtons();
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
                isInteractable = true,
            });
        }

        weaponGrid.SetItems(items, OnWeaponSlotClicked);
    }

    private void RefreshMapGrid()
    {
        if (mapGrid == null)
        {
            return;
        }

        List<InventorySlotViewData> items = new List<InventorySlotViewData>(availableMaps.Count);

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
                isInteractable = true,
            });
        }

        mapGrid.SetItems(items, OnMapSlotClicked);
    }

    private void RefreshPreview()
    {
        switch (currentTab)
        {
            case StagingTab.Weapons:
                weaponPreviewUI.ShowWeapon(selectedWeapon);
                break;

            case StagingTab.Maps:
                mapPreviewUI.ShowMap(selectedMap);
                break;

            case StagingTab.Equipment:
                // equipmentPreviewUI.ShowEquipment();
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
        RunData.SelectedWeapon = weapon;
        weaponPreviewUI.ShowWeapon(weapon);
        RefreshWeaponGrid();
    }

    private void SelectMap(MapInstance map)
    {
        selectedMap = map;
        RunData.SelectedMap = map;
        mapPreviewUI.ShowMap(map);
        RefreshMapGrid();
    }

    private void OnWeaponSlotClicked(int index, InventorySlotViewData data)
    {
        if (index < 0 || index >= allWeapons.Count)
        {
            return;
        }

        SelectWeapon(allWeapons[index]);
    }

    private void OnMapSlotClicked(int index, InventorySlotViewData data)
    {
        if (index < 0 || index >= availableMaps.Count)
        {
            return;
        }

        SelectMap(availableMaps[index]);
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
