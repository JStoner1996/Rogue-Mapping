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
    [SerializeField] private StagingOptionButtonUI buttonPrefab;
    [SerializeField] private GameObject weaponsPanel;
    [SerializeField] private GameObject mapsPanel;
    [SerializeField] private GameObject equipmentPanel;
    [SerializeField] private Transform weaponButtonParent;
    [SerializeField] private Transform mapButtonParent;
    [SerializeField] private StagingPreviewUI weaponPreviewUI;
    [SerializeField] private StagingPreviewUI mapPreviewUI;
    // [SerializeField] private StagingPreviewUI equipmentPreviewUI;
    [SerializeField] private Button weaponsTabButton;
    [SerializeField] private Button mapsTabButton;
    [SerializeField] private Button equipmentTabButton;
    [SerializeField] private Color activeTabColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color inactiveTabColor = new Color(0.17f, 0.17f, 0.17f, 0.1f);

    private readonly List<Button> tabButtons = new List<Button>();
    private readonly List<GeneratedMap> generatedMaps = new List<GeneratedMap>();
    private List<WeaponData> allWeapons = new List<WeaponData>();

    private WeaponData selectedWeapon;
    private GeneratedMap selectedMap;
    private StagingTab currentTab;

    void Start()
    {
        LoadWeapons();
        GenerateMaps();
        InitializeDefaults();
        RegisterTabButtons();
        SwitchTab(StagingTab.Weapons);
    }

    private void LoadWeapons()
    {
        allWeapons = new List<WeaponData>(Resources.LoadAll<WeaponData>("WeaponData"));
    }

    private void GenerateMaps()
    {
        generatedMaps.Clear();
        generatedMaps.AddRange(MapGenerator.GenerateChoices(4));
    }

    private void InitializeDefaults()
    {
        selectedWeapon = allWeapons.Find(w => w.weaponName == "Area Weapon");

        if (selectedWeapon == null && allWeapons.Count > 0)
        {
            selectedWeapon = allWeapons[0];
        }

        if (generatedMaps.Count > 0)
        {
            selectedMap = generatedMaps[0];
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
                ClearButtons(weaponButtonParent);

                foreach (WeaponData weapon in allWeapons)
                {
                    StagingOptionButtonUI button = Instantiate(buttonPrefab, weaponButtonParent);
                    WeaponData localWeapon = weapon;
                    button.Setup(
                        localWeapon.weaponName,
                        localWeapon.icon,
                        () => SelectWeapon(localWeapon),
                        () => weaponPreviewUI.ShowWeapon(localWeapon),
                        RefreshPreview
                    );
                }
                break;

            case StagingTab.Maps:
                ClearButtons(mapButtonParent);

                foreach (GeneratedMap map in generatedMaps)
                {
                    GeneratedMap localMap = map;
                    StagingOptionButtonUI button = Instantiate(buttonPrefab, mapButtonParent);
                    button.Setup(
                        localMap.DisplayName,
                        null,
                        () => SelectMap(localMap),
                        () => mapPreviewUI.ShowMap(localMap),
                        RefreshPreview
                    );
                }
                break;
        }
    }

    private void ClearButtons(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
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
    }

    private void SelectMap(GeneratedMap map)
    {
        selectedMap = map;
        RunData.SelectedMap = map;
        mapPreviewUI.ShowMap(map);
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
