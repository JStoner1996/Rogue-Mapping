using System.Collections.Generic;

// Coordinates the equipment tab state, UI refreshes, and event wiring.
public class EquipmentStagingController : IStagingTabController
{
    private readonly InventoryGridUI equipmentGrid;
    private readonly ItemDetailsPanelUI equipmentPreviewUI;
    private readonly PlayerStatsPanelUI playerStatsPanelUI;
    private readonly List<EquipmentSlotDropTargetUI> equipmentDropTargets;
    private readonly IEquipmentDataFacade dataFacade;
    private readonly InventoryGridInteractions gridInteractions;

    private readonly List<EquipmentInstance> availableEquipment = new List<EquipmentInstance>();
    private readonly List<string> equipmentInventoryLayout = new List<string>();

    private EquipmentInstance selectedEquipment;
    private EquipmentInstance hoveredEquipment;
    private int hoveredEquipmentIndex = -1;

    public EquipmentStagingController(
        InventoryGridUI equipmentGrid,
        ItemDetailsPanelUI equipmentPreviewUI,
        PlayerStatsPanelUI playerStatsPanelUI,
        List<EquipmentSlotDropTargetUI> equipmentDropTargets,
        IEquipmentDataFacade dataFacade = null)
    {
        this.equipmentGrid = equipmentGrid;
        this.equipmentPreviewUI = equipmentPreviewUI;
        this.playerStatsPanelUI = playerStatsPanelUI;
        this.equipmentDropTargets = equipmentDropTargets ?? new List<EquipmentSlotDropTargetUI>();
        this.dataFacade = dataFacade ?? new MetaProgressionEquipmentDataFacade();
        gridInteractions = new InventoryGridInteractions
        {
            OnSlotClicked = OnEquipmentSlotClicked,
            OnSlotRightClicked = OnEquipmentSlotRightClicked,
            OnSlotHoverEnter = OnEquipmentSlotHoverEnter,
            OnSlotHoverExit = OnEquipmentSlotHoverExit,
            CanAcceptDropAtIndex = CanAcceptEquipmentInventoryDrop,
            OnSlotDropReceived = HandleEquipmentInventorySlotDrop,
        };
    }

    public void Load()
    {
        availableEquipment.Clear();
        availableEquipment.AddRange(dataFacade.GetOwnedEquipmentInstances());
        RebuildInventoryLayout();
        selectedEquipment = availableEquipment.Find(item => item != null && item.InstanceId == selectedEquipment?.InstanceId);
        SetHoveredEquipment(null);
        playerStatsPanelUI?.Refresh();
    }

    private void RebuildInventoryLayout()
    {
        int slotCount = equipmentGrid != null ? equipmentGrid.MaxSlots : 0;
        List<string> rebuiltLayout = EquipmentInventoryLayoutService.BuildInventoryLayout(
            equipmentInventoryLayout,
            availableEquipment,
            slotCount,
            equipmentDropTargets,
            dataFacade.GetEquippedItemId);
        equipmentInventoryLayout.Clear();
        equipmentInventoryLayout.AddRange(rebuiltLayout);
    }

    public void RegisterDropTargets()
    {
        foreach (EquipmentSlotDropTargetUI dropTarget in equipmentDropTargets) BindDropTarget(dropTarget);
        RefreshEquippedSlotVisuals();
    }

    private void BindDropTarget(EquipmentSlotDropTargetUI dropTarget)
    {
        if (dropTarget == null)
        {
            return;
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

    public void RefreshGrid()
    {
        if (equipmentGrid == null)
        {
            return;
        }

        equipmentGrid.SetItems(
            EquipmentInventoryGridPresenter.BuildGridModel(
                equipmentGrid.MaxSlots,
                equipmentInventoryLayout,
                availableEquipment,
                selectedEquipment,
                hoveredEquipment,
                dataFacade),
            gridInteractions);
    }

    private void OnEquipmentSlotClicked(int index, InventorySlotModel data)
    {
        SelectEquipment(GetEquipmentAtIndex(index));
    }

    private void OnEquipmentSlotRightClicked(int index, InventorySlotModel data)
    {
        EquipmentInstance equipment = GetEquipmentAtIndex(index);
        if (equipment == null)
        {
            return;
        }

        if (!EquipmentInventoryInteractionService.ToggleEquipFromInventory(equipment, availableEquipment, equipmentDropTargets, dataFacade))
        {
            return;
        }

        HandleEquipmentMutation(equipment, rebuildLayout: true);
    }

    private void OnEquipmentSlotHoverEnter(int index, InventorySlotModel data)
    {
        ApplyHoveredEquipment(GetEquipmentAtIndex(index), index);
    }

    private void OnEquipmentSlotHoverExit(int index, InventorySlotModel data)
    {
        ApplyHoveredEquipment(null);
    }

    private bool CanAcceptEquipmentInventoryDrop(int index, DragItemPayload payload)
    {
        return EquipmentInventoryInteractionService.CanAcceptInventoryDrop(index, payload, equipmentInventoryLayout, dataFacade);
    }

    private void HandleEquipmentInventorySlotDrop(int targetIndex, DragItemPayload payload)
    {
        if (!EquipmentInventoryInteractionService.HandleInventorySlotDrop(targetIndex, payload, equipmentInventoryLayout, availableEquipment, dataFacade))
        {
            return;
        }

        HandleEquipmentMutation(rebuildLayout: false);
    }

    public void RefreshPreview()
    {
        if (equipmentPreviewUI == null)
        {
            return;
        }

        EquipmentInstance previewEquipment = hoveredEquipment ?? selectedEquipment;
        if (previewEquipment == null)
        {
            equipmentPreviewUI.ShowEquipment();
            return;
        }

        equipmentPreviewUI.ShowEquipment(previewEquipment);
    }

    public void RefreshDebugData()
    {
        Load();
        RefreshPreviewAndVisuals();
    }

    private void SelectEquipment(EquipmentInstance equipment)
    {
        selectedEquipment = equipment;
        SetHoveredEquipment(null);
        RefreshPreviewAndVisuals();
    }

    private void RefreshEquippedSlotVisuals()
    {
        EquipmentVisualStateSyncService.SyncEquippedSlotVisuals(
            equipmentDropTargets,
            selectedEquipment,
            hoveredEquipment,
            availableEquipment,
            dataFacade);
    }

    private void RefreshVisuals()
    {
        RefreshEquippedSlotVisuals();
        RefreshGrid();
    }

    private void ApplyHoveredEquipment(EquipmentInstance equipment, int inventoryIndex = -1)
    {
        SetHoveredEquipment(equipment, inventoryIndex);
        RefreshPreviewAndVisuals();
    }

    private void HandleEquippedSlotLeftClicked(EquipmentSlotDropTargetUI dropTarget)
    {
        EquipmentInstance equipment = dropTarget?.DisplayedEquipment;
        if (equipment == null)
        {
            return;
        }

        SelectEquipment(equipment);
    }

    private void HandleEquipmentSlotRightClicked(EquipmentSlotDropTargetUI dropTarget)
    {
        if (dropTarget == null)
        {
            return;
        }

        string equippedItemId = dataFacade.GetEquippedItemId(dropTarget.LoadoutSlotId);
        if (string.IsNullOrWhiteSpace(equippedItemId))
        {
            return;
        }

        if (!EquipmentInventoryInteractionService.UnequipFromLoadoutSlot(dropTarget.LoadoutSlotId, dataFacade))
        {
            return;
        }

        HandleEquipmentMutation(rebuildLayout: false);
    }

    private void HandleEquippedSlotHoverEntered(EquipmentSlotDropTargetUI dropTarget)
    {
        ApplyHoveredEquipment(dropTarget?.DisplayedEquipment);
    }

    private void HandleEquippedSlotHoverExited(EquipmentSlotDropTargetUI dropTarget)
    {
        ApplyHoveredEquipment(null);
    }

    private void HandleEquipmentDropped(EquipmentSlotDropTargetUI dropTarget, DragItemPayload payload)
    {
        if (!EquipmentInventoryInteractionService.HandleDropToEquipmentSlot(dropTarget, payload, availableEquipment, dataFacade))
        {
            return;
        }

        HandleEquipmentMutation(FindEquipmentById(payload.itemId), rebuildLayout: true);
    }

    private EquipmentInstance FindEquipmentById(string equipmentId)
    {
        if (string.IsNullOrWhiteSpace(equipmentId))
        {
            return null;
        }

        return EquipmentInstanceLookup.FindById(availableEquipment, equipmentId);
    }

    private EquipmentInstance GetEquipmentAtIndex(int index)
    {
        if (index < 0 || index >= equipmentInventoryLayout.Count)
        {
            return null;
        }

        return FindEquipmentById(equipmentInventoryLayout[index]);
    }

    private void SetHoveredEquipment(EquipmentInstance equipment, int inventoryIndex = -1)
    {
        hoveredEquipment = equipment;
        hoveredEquipmentIndex = equipment != null ? inventoryIndex : -1;
    }

    private void HandleEquipmentMutation(EquipmentInstance nextSelectedEquipment = null, bool rebuildLayout = true)
    {
        if (rebuildLayout)
        {
            RebuildInventoryLayout();
        }

        selectedEquipment = nextSelectedEquipment ?? FindEquipmentById(selectedEquipment?.InstanceId);
        RestoreHoveredInventoryEquipment();
        RefreshPreviewAndVisuals();
        playerStatsPanelUI?.Refresh();
    }

    private void RefreshPreviewAndVisuals()
    {
        RefreshPreview();
        RefreshVisuals();
    }

    private void RestoreHoveredInventoryEquipment()
    {
        if (hoveredEquipmentIndex < 0 || hoveredEquipmentIndex >= equipmentInventoryLayout.Count)
        {
            SetHoveredEquipment(null);
            return;
        }

        // Inventory hover state can survive layout rebuilds only if the same slot still resolves to a live item.
        SetHoveredEquipment(GetEquipmentAtIndex(hoveredEquipmentIndex), hoveredEquipmentIndex);
    }
}
