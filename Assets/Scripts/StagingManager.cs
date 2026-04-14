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
    private static readonly EquipmentSlotType[] equipmentInventoryPriorityOrder =
    {
        EquipmentSlotType.Head,
        EquipmentSlotType.Chest,
        EquipmentSlotType.Legs,
        EquipmentSlotType.Feet,
        EquipmentSlotType.Hands,
        EquipmentSlotType.Necklace,
        EquipmentSlotType.Ring,
        EquipmentSlotType.Ring,
    };
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

        weaponGrid.SetItems(items, OnWeaponSlotClicked, null, OnWeaponSlotHoverEnter, OnWeaponSlotHoverExit);
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

        mapGrid.SetItems(items, OnMapSlotClicked, null, OnMapSlotHoverEnter, OnMapSlotHoverExit);
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
        List<InventorySlotViewData> items = new List<InventorySlotViewData>(slotCount);
        EquipmentInstance focusedEquipment = hoveredEquipment != null ? hoveredEquipment : selectedEquipment;

        for (int i = 0; i < slotCount; i++)
        {
            string equipmentId = i < equipmentInventoryLayout.Count ? equipmentInventoryLayout[i] : string.Empty;
            EquipmentInstance equipment = FindEquipmentById(equipmentId);

            if (equipment == null)
            {
                items.Add(InventorySlotViewData.Empty());
                continue;
            }

            bool isEquipped = IsEquipmentEquipped(equipment.InstanceId);

            items.Add(new InventorySlotViewData
            {
                id = equipment.InstanceId,
                label = equipment.DisplayName,
                icon = equipment.Icon,
                isEmpty = false,
                isSelected = equipment == selectedEquipment,
                isFocused = equipment == focusedEquipment,
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
            items,
            OnEquipmentSlotClicked,
            OnEquipmentSlotRightClicked,
            OnEquipmentSlotHoverEnter,
            OnEquipmentSlotHoverExit,
            CanAcceptEquipmentInventoryDrop,
            HandleEquipmentInventorySlotDrop);
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
        RebuildEquipmentInventoryLayout();
        SelectEquipment(equipment);
        RefreshHoveredEquipmentFromCurrentIndex();
        RefreshEquippedSlotVisuals();
        RefreshEquipmentGrid();
        RefreshPlayerStatsPanel();
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
        RefreshHoveredEquipmentFromCurrentIndex();
        RefreshEquippedSlotVisuals();
        RefreshEquipmentGrid();
        RefreshPlayerStatsPanel();
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

    private void RefreshPlayerStatsPanel()
    {
        if (playerStatsPanelUI != null)
        {
            playerStatsPanelUI.Refresh();
        }
    }

    private bool IsEquipmentEquipped(string equipmentInstanceId)
    {
        if (string.IsNullOrWhiteSpace(equipmentInstanceId))
        {
            return false;
        }

        EquipmentLoadoutData loadout = MetaProgressionService.GetEquipmentLoadout();

        if (loadout?.equippedItems == null)
        {
            return false;
        }

        for (int i = 0; i < loadout.equippedItems.Count; i++)
        {
            EquipmentLoadoutSlot slot = loadout.equippedItems[i];

            if (slot != null && slot.equipmentInstanceId == equipmentInstanceId)
            {
                return true;
            }
        }

        return false;
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

    private void OnEquipmentSlotRightClicked(int index, InventorySlotViewData data)
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

        if (IsEquipmentEquipped(equipment.InstanceId))
        {
            UnequipItem(equipment.InstanceId);
            RebuildEquipmentInventoryLayout();
            RefreshHoveredEquipmentFromCurrentIndex();
            RefreshEquippedSlotVisuals();
            RefreshEquipmentGrid();
            RefreshPlayerStatsPanel();
            return;
        }

        SelectEquipment(equipment);
        EquipmentSlotDropTargetUI targetSlot = FindBestEquipTarget(equipment);

        if (targetSlot == null)
        {
            return;
        }

        MetaProgressionService.SetEquippedItem(targetSlot.LoadoutSlotId, equipment.InstanceId);
        RebuildEquipmentInventoryLayout();
        RefreshHoveredEquipmentFromCurrentIndex();
        RefreshEquippedSlotVisuals();
        RefreshEquipmentGrid();
        RefreshPlayerStatsPanel();
    }

    private void OnEquipmentSlotHoverEnter(int index, InventorySlotViewData data)
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
        RefreshHoveredEquipmentFromCurrentIndex();
        RefreshEquippedSlotVisuals();
        RefreshEquipmentGrid();
        RefreshPlayerStatsPanel();
    }

    private void OnEquipmentSlotHoverExit(int index, InventorySlotViewData data)
    {
        hoveredEquipment = null;
        hoveredEquipmentIndex = -1;
        RefreshPreview();
        RefreshEquipmentGrid();
    }

    private EquipmentSlotDropTargetUI FindBestEquipTarget(EquipmentInstance equipment)
    {
        if (equipment == null)
        {
            return null;
        }

        List<EquipmentSlotDropTargetUI> candidates = new List<EquipmentSlotDropTargetUI>();

        for (int i = 0; i < equipmentDropTargets.Count; i++)
        {
            EquipmentSlotDropTargetUI dropTarget = equipmentDropTargets[i];
            if (dropTarget != null && dropTarget.SlotType == equipment.SlotType)
            {
                candidates.Add(dropTarget);
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        if (candidates.Count == 1)
        {
            return candidates[0];
        }

        EquipmentSlotDropTargetUI emptySlot = null;
        EquipmentSlotDropTargetUI lowerTierSlot = null;
        EquipmentSlotDropTargetUI lowerRaritySlot = null;
        EquipmentSlotDropTargetUI firstSlot = candidates[0];

        for (int i = 0; i < candidates.Count; i++)
        {
            EquipmentSlotDropTargetUI candidate = candidates[i];
            string equippedItemId = MetaProgressionService.GetEquippedItemId(candidate.LoadoutSlotId);

            if (string.IsNullOrWhiteSpace(equippedItemId))
            {
                emptySlot = candidate;
                break;
            }

            EquipmentInstance equippedItem = availableEquipment.Find(item => item != null && item.InstanceId == equippedItemId);
            if (equippedItem == null)
            {
                emptySlot = candidate;
                break;
            }

            if (equippedItem.InstanceId == equipment.InstanceId)
            {
                continue;
            }

            if (lowerTierSlot == null && equippedItem.ItemTier < equipment.ItemTier)
            {
                lowerTierSlot = candidate;
            }

            if (lowerRaritySlot == null && CompareRarity(equippedItem.Rarity, equipment.Rarity) < 0)
            {
                lowerRaritySlot = candidate;
            }
        }

        if (emptySlot != null)
        {
            return emptySlot;
        }

        if (lowerTierSlot != null)
        {
            return lowerTierSlot;
        }

        if (lowerRaritySlot != null)
        {
            return lowerRaritySlot;
        }

        return firstSlot;
    }

    private int CompareRarity(EquipmentRarity left, EquipmentRarity right)
    {
        return left.CompareTo(right);
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

        if (!string.IsNullOrWhiteSpace(targetItemId) && IsEquipmentEquipped(targetItemId))
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
            string targetItemId = equipmentInventoryLayout[targetIndex];
            equipmentInventoryLayout[targetIndex] = equipmentInventoryLayout[sourceIndex];
            equipmentInventoryLayout[sourceIndex] = targetItemId;
        }

        RefreshEquippedSlotVisuals();
        RefreshEquipmentGrid();
        RefreshPlayerStatsPanel();
    }

    private void RebuildEquipmentInventoryLayout()
    {
        List<string> previousLayout = new List<string>(equipmentInventoryLayout);
        int slotCount = equipmentGrid != null ? equipmentGrid.MaxSlots : 0;
        slotCount = Mathf.Max(slotCount, 8);
        slotCount = Mathf.Max(slotCount, availableEquipment.Count);

        List<string> availableIds = new List<string>(availableEquipment.Count);
        HashSet<string> availableIdSet = new HashSet<string>();

        for (int i = 0; i < availableEquipment.Count; i++)
        {
            if (availableEquipment[i] == null || string.IsNullOrWhiteSpace(availableEquipment[i].InstanceId))
            {
                continue;
            }

            if (availableIdSet.Add(availableEquipment[i].InstanceId))
            {
                availableIds.Add(availableEquipment[i].InstanceId);
            }
        }

        List<string> equippedPriorityIds = GetEquippedPriorityIds();
        HashSet<string> equippedIdSet = new HashSet<string>(equippedPriorityIds);

        equipmentInventoryLayout.Clear();
        for (int i = 0; i < slotCount; i++)
        {
            string previousId = i < previousLayout.Count ? previousLayout[i] : string.Empty;
            equipmentInventoryLayout.Add(availableIdSet.Contains(previousId) ? previousId : string.Empty);
        }

        // Remove equipped items from their old positions first so their previous slots become holes.
        for (int i = 0; i < equipmentInventoryLayout.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(equipmentInventoryLayout[i]) && equippedIdSet.Contains(equipmentInventoryLayout[i]))
            {
                equipmentInventoryLayout[i] = string.Empty;
            }
        }

        List<string> displacedIds = new List<string>();

        // Pin the currently equipped items into the compact priority strip.
        for (int i = 0; i < equippedPriorityIds.Count; i++)
        {
            EnsureEquipmentInventoryLayoutSize(i + 1);

            string displacedId = equipmentInventoryLayout[i];
            if (!string.IsNullOrWhiteSpace(displacedId) && !equippedIdSet.Contains(displacedId))
            {
                displacedIds.Add(displacedId);
            }

            equipmentInventoryLayout[i] = equippedPriorityIds[i];
        }

        // Put any directly displaced items into the next open holes without disturbing the rest.
        for (int i = 0; i < displacedIds.Count; i++)
        {
            int nextEmptyIndex = FindNextEmptyEquipmentInventoryIndex();
            equipmentInventoryLayout[nextEmptyIndex] = displacedIds[i];
        }

        HashSet<string> placedIds = new HashSet<string>();
        for (int i = 0; i < equipmentInventoryLayout.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(equipmentInventoryLayout[i]))
            {
                placedIds.Add(equipmentInventoryLayout[i]);
            }
        }

        // Any brand new items that were not in the previous layout yet get appended into the next empty holes.
        for (int i = 0; i < availableIds.Count; i++)
        {
            if (placedIds.Contains(availableIds[i]))
            {
                continue;
            }

            int nextEmptyIndex = FindNextEmptyEquipmentInventoryIndex();
            equipmentInventoryLayout[nextEmptyIndex] = availableIds[i];
        }
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

    private List<string> GetEquippedPriorityIds()
    {
        List<string> equippedIds = new List<string>(equipmentInventoryPriorityOrder.Length);
        List<EquipmentSlotDropTargetUI> orderedTargets = new List<EquipmentSlotDropTargetUI>(equipmentDropTargets);
        orderedTargets.Sort(CompareEquipmentDropTargetPriority);

        for (int i = 0; i < orderedTargets.Count; i++)
        {
            EquipmentSlotDropTargetUI dropTarget = orderedTargets[i];

            if (dropTarget == null)
            {
                continue;
            }

            string equippedItemId = MetaProgressionService.GetEquippedItemId(dropTarget.LoadoutSlotId);
            if (!string.IsNullOrWhiteSpace(equippedItemId))
            {
                equippedIds.Add(equippedItemId);
            }
        }

        return equippedIds;
    }

    private int CompareEquipmentDropTargetPriority(EquipmentSlotDropTargetUI left, EquipmentSlotDropTargetUI right)
    {
        int leftPriority = GetEquipmentDropTargetPriority(left);
        int rightPriority = GetEquipmentDropTargetPriority(right);

        if (leftPriority != rightPriority)
        {
            return leftPriority.CompareTo(rightPriority);
        }

        string leftId = left != null ? left.LoadoutSlotId : string.Empty;
        string rightId = right != null ? right.LoadoutSlotId : string.Empty;
        return string.CompareOrdinal(leftId, rightId);
    }

    private int GetEquipmentDropTargetPriority(EquipmentSlotDropTargetUI dropTarget)
    {
        if (dropTarget == null)
        {
            return int.MaxValue;
        }

        for (int i = 0; i < equipmentInventoryPriorityOrder.Length; i++)
        {
            if (equipmentInventoryPriorityOrder[i] != dropTarget.SlotType)
            {
                continue;
            }

            if (dropTarget.SlotType != EquipmentSlotType.Ring)
            {
                return i;
            }

            bool isRingTwo = dropTarget.LoadoutSlotId.IndexOf("2", System.StringComparison.OrdinalIgnoreCase) >= 0;
            return isRingTwo ? 7 : 6;
        }

        return int.MaxValue - 1;
    }

    private bool IsEquipmentInventorySlotEmpty(int index)
    {
        return index >= 0
            && index < equipmentInventoryLayout.Count
            && string.IsNullOrWhiteSpace(equipmentInventoryLayout[index]);
    }

    private int FindNextEmptyEquipmentInventoryIndex()
    {
        for (int i = 0; i < equipmentInventoryLayout.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(equipmentInventoryLayout[i]))
            {
                return i;
            }
        }

        equipmentInventoryLayout.Add(string.Empty);
        return equipmentInventoryLayout.Count - 1;
    }

    private void EnsureEquipmentInventoryLayoutSize(int requiredSize)
    {
        while (equipmentInventoryLayout.Count < requiredSize)
        {
            equipmentInventoryLayout.Add(string.Empty);
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
