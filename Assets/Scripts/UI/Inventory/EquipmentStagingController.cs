using System.Collections.Generic;
using UnityEngine;

public class EquipmentStagingController : IStagingTabController
{
    private readonly InventoryGridUI equipmentGrid;
    private readonly ItemDetailsPanelUI equipmentPreviewUI;
    private readonly PlayerStatsPanelUI playerStatsPanelUI;
    private readonly List<EquipmentSlotDropTargetUI> equipmentDropTargets;

    private readonly List<EquipmentInstance> availableEquipment = new List<EquipmentInstance>();
    private readonly List<string> equipmentInventoryLayout = new List<string>();

    private EquipmentInstance selectedEquipment;
    private EquipmentInstance hoveredEquipment;
    private int hoveredEquipmentIndex = -1;

    public EquipmentStagingController(
        InventoryGridUI equipmentGrid,
        ItemDetailsPanelUI equipmentPreviewUI,
        PlayerStatsPanelUI playerStatsPanelUI,
        List<EquipmentSlotDropTargetUI> equipmentDropTargets)
    {
        this.equipmentGrid = equipmentGrid;
        this.equipmentPreviewUI = equipmentPreviewUI;
        this.playerStatsPanelUI = playerStatsPanelUI;
        this.equipmentDropTargets = equipmentDropTargets ?? new List<EquipmentSlotDropTargetUI>();
    }

    public void Load()
    {
        availableEquipment.Clear();
        availableEquipment.AddRange(MetaProgressionService.GetOwnedEquipmentInstances());
        RebuildInventoryLayout();
        selectedEquipment = availableEquipment.Find(item => item != null && item.InstanceId == selectedEquipment?.InstanceId);
        hoveredEquipment = null;
        hoveredEquipmentIndex = -1;
        RefreshPlayerStatsPanel();
    }

    public void RegisterDropTargets()
    {
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

    public void RefreshGrid()
    {
        if (equipmentGrid == null)
        {
            return;
        }

        int slotCount = Mathf.Max(equipmentGrid.MaxSlots, equipmentInventoryLayout.Count);
        List<InventorySlotModel> items = new List<InventorySlotModel>(slotCount);
        EquipmentLoadoutData loadout = MetaProgressionService.GetEquipmentLoadout();

        for (int i = 0; i < slotCount; i++)
        {
            string equipmentId = i < equipmentInventoryLayout.Count ? equipmentInventoryLayout[i] : string.Empty;
            EquipmentInstance equipment = FindEquipmentById(equipmentId);

            if (equipment == null)
            {
                items.Add(InventorySlotModel.Empty());
                continue;
            }

            bool isEquipped = EquipmentInventoryLayoutService.IsEquipped(equipment.InstanceId, loadout);

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

    public void RefreshPreview()
    {
        if (equipmentPreviewUI == null)
        {
            return;
        }

        if (hoveredEquipment != null || selectedEquipment != null)
        {
            equipmentPreviewUI.ShowEquipment(hoveredEquipment ?? selectedEquipment);
        }
        else
        {
            equipmentPreviewUI.ShowEquipment();
        }
    }

    public void RefreshDebugData()
    {
        Load();
        RefreshEquippedSlotVisuals();
        RefreshGrid();
        RefreshPlayerStatsPanel();
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
        RefreshGrid();
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
            EquipmentInstance equippedItem = FindEquipmentById(equippedItemId);
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

    private void RefreshPresentation()
    {
        RefreshHoveredEquipmentFromCurrentIndex();
        RefreshEquippedSlotVisuals();
        RefreshGrid();
        RefreshPlayerStatsPanel();
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
            RebuildInventoryLayout();
            RefreshPresentation();
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
        RebuildInventoryLayout();
        selectedEquipment = equipment;
        RefreshPresentation();
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
        RefreshGrid();
    }

    private void OnEquipmentSlotHoverExit(int index, InventorySlotModel data)
    {
        hoveredEquipment = null;
        hoveredEquipmentIndex = -1;
        RefreshPreview();
        RefreshEquippedSlotVisuals();
        RefreshGrid();
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
        RefreshPresentation();
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
        RefreshGrid();
    }

    private void HandleEquippedSlotHoverExited(EquipmentSlotDropTargetUI dropTarget)
    {
        hoveredEquipment = null;
        hoveredEquipmentIndex = -1;
        RefreshPreview();
        RefreshEquippedSlotVisuals();
        RefreshGrid();
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
                RebuildInventoryLayout();
                selectedEquipment = equipment;
                RefreshPresentation();
                return;
            }
        }

        MetaProgressionService.SetEquippedItem(dropTarget.LoadoutSlotId, equipment.InstanceId);
        RebuildInventoryLayout();
        selectedEquipment = equipment;
        RefreshPresentation();
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
            RefreshPresentation();
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

        RefreshPresentation();
    }

    private void RebuildInventoryLayout()
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
}
