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
    private readonly List<MapInstance> availableMaps = new List<MapInstance>();
    private readonly List<EquipmentInstance> availableEquipment = new List<EquipmentInstance>();
    private readonly List<string> equipmentInventoryLayout = new List<string>();
    private List<WeaponData> allWeapons = new List<WeaponData>();

    private WeaponData selectedWeapon;
    private MapInstance selectedMap;
    private EquipmentInstance selectedEquipment;
    private WeaponData hoveredWeapon;
    private MapInstance hoveredMap;
    private EquipmentInstance hoveredEquipment;
    private int hoveredEquipmentIndex = -1;
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
        RefreshPlayerStatsPanel();
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
        RebuildEquipmentInventoryLayout();
        selectedEquipment = availableEquipment.Find(item => item != null && item.InstanceId == selectedEquipment?.InstanceId);
        hoveredEquipment = null;
        hoveredEquipmentIndex = -1;
        RefreshPlayerStatsPanel();
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

        selectedEquipment = null;

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
        if (equipmentInventoryDropTarget != null)
        {
            equipmentInventoryDropTarget.DropReceived -= HandleEquipmentDroppedBackToInventory;
            equipmentInventoryDropTarget.DropReceived += HandleEquipmentDroppedBackToInventory;
        }

        foreach (EquipmentSlotDropTargetUI dropTarget in equipmentDropTargets)
        {
            if (dropTarget == null)
            {
                continue;
            }

            dropTarget.DropReceived -= HandleEquipmentDropped;
            dropTarget.DropReceived += HandleEquipmentDropped;
            dropTarget.RightClicked -= HandleEquipmentSlotRightClicked;
            dropTarget.RightClicked += HandleEquipmentSlotRightClicked;
            dropTarget.LeftClicked -= HandleEquippedSlotLeftClicked;
            dropTarget.LeftClicked += HandleEquippedSlotLeftClicked;
            dropTarget.HoverEntered -= HandleEquippedSlotHoverEntered;
            dropTarget.HoverEntered += HandleEquippedSlotHoverEntered;
            dropTarget.HoverExited -= HandleEquippedSlotHoverExited;
            dropTarget.HoverExited += HandleEquippedSlotHoverExited;
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

    private void RefreshMapGrid()
    {
        if (mapGrid == null)
        {
            return;
        }

        List<InventorySlotModel> items = new List<InventorySlotModel>(availableMaps.Count);
        foreach (MapInstance map in availableMaps)
        {
            if (map == null)
            {
                continue;
            }

            items.Add(new InventorySlotModel
            {
                id = map.BaseMapId + "|" + map.DisplayName,
                label = map.DisplayName,
                icon = map.Icon,
                isEmpty = false,
                isSelected = map == selectedMap,
                isHovered = map == hoveredMap,
                isInteractable = true,
            });
        }

        mapGrid.SetItems(
            new InventoryGridModel(items, mapGrid.MaxSlots),
            new InventoryGridInteractions
            {
                OnSlotClicked = OnMapSlotClicked,
                OnSlotHoverEnter = OnMapSlotHoverEnter,
                OnSlotHoverExit = OnMapSlotHoverExit,
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

        int slotCount = Mathf.Max(equipmentGrid.MaxSlots, equipmentInventoryLayout.Count);
        List<InventorySlotModel> items = new List<InventorySlotModel>(slotCount);
        for (int i = 0; i < slotCount; i++)
        {
            string equipmentId = i < equipmentInventoryLayout.Count ? equipmentInventoryLayout[i] : string.Empty;
            EquipmentInstance equipment = FindEquipmentById(equipmentId);

            if (equipment == null)
            {
                items.Add(InventorySlotModel.Empty());
                continue;
            }

            bool isEquipped = EquipmentInventoryLayoutService.IsEquipped(equipment.InstanceId, MetaProgressionService.GetEquipmentLoadout());

            items.Add(new InventorySlotModel
            {
                id = equipment.InstanceId,
                label = equipment.DisplayName,
                icon = equipment.Icon,
                iconTint = GetEquipmentTierTint(equipment.ItemTier),
                isEmpty = false,
                isSelected = equipment == selectedEquipment,
                isHovered = equipment == hoveredEquipment,
                isEquipped = isEquipped,
                isInteractable = true,
                canDrag = !isEquipped,
                dragItemType = DragItemType.Equipment,
                dragItemSourceType = DragItemSourceType.Inventory,
                hasEquipmentSlotType = true,
                equipmentSlotType = equipment.SlotType,
            });
        }

        equipmentGrid.SetItems(
            new InventoryGridModel(items, equipmentGrid.MaxSlots),
            new InventoryGridInteractions
            {
                OnSlotClicked = OnEquipmentSlotClicked,
                OnSlotRightClicked = OnEquipmentSlotRightClicked,
                OnSlotHoverEnter = OnEquipmentSlotHoverEnter,
                OnSlotHoverExit = OnEquipmentSlotHoverExit,
                CanAcceptDropAtIndex = CanAcceptEquipmentInventoryDrop,
                OnSlotDropReceived = HandleEquipmentInventorySlotDrop,
            });
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
        hoveredEquipmentIndex = -1;
        if (equipmentPreviewUI != null)
        {
            equipmentPreviewUI.ShowEquipment(equipment);
        }

        RefreshEquippedSlotVisuals();
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

        if (payload.sourceType == DragItemSourceType.EquippedSlot
            && !string.IsNullOrWhiteSpace(payload.sourceSlotId)
            && payload.sourceSlotId != dropTarget.LoadoutSlotId)
        {
            string targetEquippedItemId = MetaProgressionService.GetEquippedItemId(dropTarget.LoadoutSlotId);
            EquipmentInstance targetEquippedItem = FindEquipmentById(targetEquippedItemId);

            if (targetEquippedItem != null && targetEquippedItem.SlotType == equipment.SlotType)
            {
                MetaProgressionService.SetEquippedItem(payload.sourceSlotId, targetEquippedItem.InstanceId, false);
                MetaProgressionService.SetEquippedItem(dropTarget.LoadoutSlotId, equipment.InstanceId, false);
                MetaProgressionService.Save();
                RebuildEquipmentInventoryLayout();
                selectedEquipment = equipment;
                RefreshEquipmentPresentation();
                return;
            }
        }

        MetaProgressionService.SetEquippedItem(dropTarget.LoadoutSlotId, equipment.InstanceId);
        RebuildEquipmentInventoryLayout();
        selectedEquipment = equipment;
        RefreshEquipmentPresentation();
    }

    private void HandleEquipmentSlotRightClicked(EquipmentSlotDropTargetUI dropTarget)
    {
        if (dropTarget == null)
        {
            return;
        }

        string equippedItemId = MetaProgressionService.GetEquippedItemId(dropTarget.LoadoutSlotId);
        if (string.IsNullOrWhiteSpace(equippedItemId))
        {
            return;
        }

        MetaProgressionService.SetEquippedItem(dropTarget.LoadoutSlotId, string.Empty);
        RefreshEquipmentPresentation();
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
            dropTarget.SetSelected(equippedItem != null && selectedEquipment != null && equippedItem.InstanceId == selectedEquipment.InstanceId);
            dropTarget.SetHovered(equippedItem != null && hoveredEquipment != null && equippedItem.InstanceId == hoveredEquipment.InstanceId);
        }
    }

    private void RefreshPlayerStatsPanel()
    {
        if (playerStatsPanelUI != null)
        {
            playerStatsPanelUI.Refresh();
        }
    }

    private void RefreshEquipmentPresentation()
    {
        RefreshHoveredEquipmentFromCurrentIndex();
        RefreshEquippedSlotVisuals();
        RefreshEquipmentGrid();
        RefreshPlayerStatsPanel();
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

    private void OnMapSlotClicked(int index, InventorySlotModel data)
    {
        if (index < 0 || index >= availableMaps.Count)
        {
            return;
        }

        SelectMap(availableMaps[index]);
    }

    private void OnMapSlotHoverEnter(int index, InventorySlotModel data)
    {
        if (index < 0 || index >= availableMaps.Count)
        {
            return;
        }

        hoveredMap = availableMaps[index];
        mapPreviewUI.ShowMap(hoveredMap);
        RefreshMapGrid();
    }

    private void OnMapSlotHoverExit(int index, InventorySlotModel data)
    {
        hoveredMap = null;
        RefreshPreview();
        RefreshMapGrid();
    }

    private void OnEquipmentSlotClicked(int index, InventorySlotModel data)
    {
        if (index < 0 || index >= equipmentInventoryLayout.Count)
        {
            return;
        }

        EquipmentInstance equipment = FindEquipmentById(equipmentInventoryLayout[index]);
        if (equipment != null)
        {
            SelectEquipment(equipment);
        }
    }

    private void OnEquipmentSlotRightClicked(int index, InventorySlotModel data)
    {
        if (index < 0 || index >= equipmentInventoryLayout.Count)
        {
            return;
        }

        EquipmentInstance equipment = FindEquipmentById(equipmentInventoryLayout[index]);
        if (equipment == null)
        {
            return;
        }

        if (EquipmentInventoryLayoutService.IsEquipped(equipment.InstanceId, MetaProgressionService.GetEquipmentLoadout()))
        {
            UnequipItem(equipment.InstanceId);
            RebuildEquipmentInventoryLayout();
            RefreshEquipmentPresentation();
            return;
        }

        EquipmentSlotDropTargetUI targetSlot = EquipmentInventoryLayoutService.FindBestEquipTarget(
            equipment,
            equipmentDropTargets,
            availableEquipment,
            MetaProgressionService.GetEquippedItemId);

        if (targetSlot == null)
        {
            return;
        }

        MetaProgressionService.SetEquippedItem(targetSlot.LoadoutSlotId, equipment.InstanceId);
        RebuildEquipmentInventoryLayout();
        selectedEquipment = equipment;
        RefreshEquipmentPresentation();
    }

    private void OnEquipmentSlotHoverEnter(int index, InventorySlotModel data)
    {
        if (index < 0 || index >= equipmentInventoryLayout.Count)
        {
            return;
        }

        hoveredEquipment = FindEquipmentById(equipmentInventoryLayout[index]);
        hoveredEquipmentIndex = index;
        if (hoveredEquipment == null)
        {
            return;
        }

        if (equipmentPreviewUI != null)
        {
            equipmentPreviewUI.ShowEquipment(hoveredEquipment);
        }

        RefreshEquippedSlotVisuals();
        RefreshEquipmentGrid();
    }

    private void HandleEquipmentDroppedBackToInventory(DragItemPayload payload)
    {
        if (payload == null || string.IsNullOrWhiteSpace(payload.itemId))
        {
            return;
        }

        UnequipItem(payload.itemId);
        RebuildEquipmentInventoryLayout();
        RefreshEquipmentPresentation();
    }

    private void HandleEquippedSlotLeftClicked(EquipmentSlotDropTargetUI dropTarget)
    {
        if (dropTarget == null || dropTarget.DisplayedEquipment == null)
        {
            return;
        }

        SelectEquipment(dropTarget.DisplayedEquipment);
        RefreshEquippedSlotVisuals();
    }

    private void HandleEquippedSlotHoverEntered(EquipmentSlotDropTargetUI dropTarget)
    {
        if (dropTarget == null || dropTarget.DisplayedEquipment == null)
        {
            return;
        }

        hoveredEquipment = dropTarget.DisplayedEquipment;
        hoveredEquipmentIndex = -1;
        RefreshPreview();
        RefreshEquippedSlotVisuals();
        RefreshEquipmentGrid();
    }

    private void HandleEquippedSlotHoverExited(EquipmentSlotDropTargetUI dropTarget)
    {
        hoveredEquipment = null;
        hoveredEquipmentIndex = -1;
        RefreshPreview();
        RefreshEquippedSlotVisuals();
        RefreshEquipmentGrid();
    }

    private void OnEquipmentSlotHoverExit(int index, InventorySlotModel data)
    {
        hoveredEquipment = null;
        hoveredEquipmentIndex = -1;
        RefreshPreview();
        RefreshEquippedSlotVisuals();
        RefreshEquipmentGrid();
    }

    private bool CanAcceptEquipmentInventoryDrop(int index, DragItemPayload payload)
    {
        if (payload == null
            || payload.itemType != DragItemType.Equipment
            || index < 0
            || index >= equipmentInventoryLayout.Count)
        {
            return false;
        }

        string targetItemId = equipmentInventoryLayout[index];

        if (!string.IsNullOrWhiteSpace(targetItemId) && EquipmentInventoryLayoutService.IsEquipped(targetItemId, MetaProgressionService.GetEquipmentLoadout()))
        {
            return false;
        }

        return true;
    }

    private void HandleEquipmentInventorySlotDrop(int targetIndex, DragItemPayload payload)
    {
        if (!CanAcceptEquipmentInventoryDrop(targetIndex, payload) || string.IsNullOrWhiteSpace(payload.itemId))
        {
            return;
        }

        int sourceIndex = equipmentInventoryLayout.FindIndex(id => id == payload.itemId);
        string targetItemId = targetIndex >= 0 && targetIndex < equipmentInventoryLayout.Count
            ? equipmentInventoryLayout[targetIndex]
            : string.Empty;
        EquipmentInstance targetEquipment = FindEquipmentById(targetItemId);

        if (payload.sourceType == DragItemSourceType.EquippedSlot
            && targetEquipment != null
            && !string.IsNullOrWhiteSpace(payload.sourceSlotId)
            && targetEquipment.SlotType == payload.equipmentSlotType
            && targetEquipment.InstanceId != payload.itemId)
        {
            MetaProgressionService.SetEquippedItem(payload.sourceSlotId, targetEquipment.InstanceId);

            if (sourceIndex >= 0)
            {
                equipmentInventoryLayout[sourceIndex] = targetEquipment.InstanceId;
            }

            equipmentInventoryLayout[targetIndex] = payload.itemId;
            RefreshEquipmentPresentation();
            return;
        }

        if (sourceIndex < 0)
        {
            return;
        }

        if (payload.sourceType == DragItemSourceType.EquippedSlot)
        {
            UnequipItem(payload.itemId);
        }

        if (sourceIndex != targetIndex)
        {
            equipmentInventoryLayout[targetIndex] = equipmentInventoryLayout[sourceIndex];
            equipmentInventoryLayout[sourceIndex] = targetItemId;
        }

        RefreshEquipmentPresentation();
    }

    private void RebuildEquipmentInventoryLayout()
    {
        int slotCount = equipmentGrid != null ? equipmentGrid.MaxSlots : 0;
        List<string> rebuiltLayout = EquipmentInventoryLayoutService.BuildInventoryLayout(
            equipmentInventoryLayout,
            availableEquipment,
            slotCount,
            equipmentDropTargets,
            MetaProgressionService.GetEquippedItemId);
        equipmentInventoryLayout.Clear();
        equipmentInventoryLayout.AddRange(rebuiltLayout);
    }

    private EquipmentInstance FindEquipmentById(string equipmentId)
    {
        if (string.IsNullOrWhiteSpace(equipmentId))
        {
            return null;
        }

        return availableEquipment.Find(item => item != null && item.InstanceId == equipmentId);
    }

    private void UnequipItem(string equipmentInstanceId)
    {
        if (string.IsNullOrWhiteSpace(equipmentInstanceId))
        {
            return;
        }

        EquipmentLoadoutData loadout = MetaProgressionService.GetEquipmentLoadout();
        bool changed = false;

        if (loadout?.equippedItems != null)
        {
            for (int i = 0; i < loadout.equippedItems.Count; i++)
            {
                EquipmentLoadoutSlot slot = loadout.equippedItems[i];

                if (slot != null && slot.equipmentInstanceId == equipmentInstanceId)
                {
                    MetaProgressionService.SetEquippedItem(slot.slotId, string.Empty, false);
                    changed = true;
                }
            }
        }

        if (changed)
        {
            MetaProgressionService.Save();
        }
    }

    private void RefreshHoveredEquipmentFromCurrentIndex()
    {
        if (hoveredEquipmentIndex < 0 || hoveredEquipmentIndex >= equipmentInventoryLayout.Count)
        {
            hoveredEquipment = null;
            hoveredEquipmentIndex = -1;
            RefreshPreview();
            return;
        }

        hoveredEquipment = FindEquipmentById(equipmentInventoryLayout[hoveredEquipmentIndex]);
        RefreshPreview();
    }

    private Color GetEquipmentTierTint(int itemTier)
    {
        float normalizedTier = Mathf.InverseLerp(1f, 10f, Mathf.Clamp(itemTier, 1, 10));
        return Color.Lerp(Color.white, Color.red, normalizedTier);
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

    public void RefreshEquipmentDebugData()
    {
        LoadEquipment();
        RefreshEquippedSlotVisuals();
        RefreshEquipmentGrid();
        RefreshPlayerStatsPanel();
    }
}
