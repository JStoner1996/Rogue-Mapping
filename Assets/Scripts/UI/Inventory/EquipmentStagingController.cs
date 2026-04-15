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

        if (!EquipmentInventoryInteractionService.ToggleEquipFromInventory(equipment, availableEquipment, equipmentDropTargets))
        {
            return;
        }

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

        if (!EquipmentInventoryInteractionService.UnequipFromLoadoutSlot(dropTarget.LoadoutSlotId))
        {
            return;
        }

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
        if (!EquipmentInventoryInteractionService.HandleDropToEquipmentSlot(dropTarget, payload, availableEquipment))
        {
            return;
        }

        EquipmentInstance equipment = FindEquipmentById(payload.itemId);
        RebuildInventoryLayout();
        selectedEquipment = equipment;
        RefreshPresentation();
    }

    private bool CanAcceptEquipmentInventoryDrop(int index, DragItemPayload payload)
    {
        return EquipmentInventoryInteractionService.CanAcceptInventoryDrop(index, payload, equipmentInventoryLayout);
    }

    private void HandleEquipmentInventorySlotDrop(int targetIndex, DragItemPayload payload)
    {
        if (!EquipmentInventoryInteractionService.HandleInventorySlotDrop(targetIndex, payload, equipmentInventoryLayout, availableEquipment))
        {
            return;
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
