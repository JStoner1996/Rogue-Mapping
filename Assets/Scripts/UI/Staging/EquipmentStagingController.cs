using System.Collections.Generic;

// Coordinates the equipment tab state, UI refreshes, and event wiring.
public class EquipmentStagingController : IStagingTabController
{
    private readonly InventoryGridUI equipmentGrid;
    private readonly ItemDetailsPanelUI equipmentPreviewUI;
    private readonly PlayerStatsPanelUI playerStatsPanelUI;
    private readonly List<EquipmentSlotDropTargetUI> equipmentDropTargets;
    private readonly IEquipmentDataFacade dataFacade;

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
    }

    public void Load()
    {
        availableEquipment.Clear();
        availableEquipment.AddRange(dataFacade.GetOwnedEquipmentInstances());
        RebuildInventoryLayout();
        selectedEquipment = availableEquipment.Find(item => item != null && item.InstanceId == selectedEquipment?.InstanceId);
        ClearHoveredEquipment();
        RefreshPlayerStatsPanel();
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
        foreach (EquipmentSlotDropTargetUI dropTarget in equipmentDropTargets)
        {
            RegisterDropTarget(dropTarget);
        }

        RefreshEquippedSlotVisuals();
    }

    private void RegisterDropTarget(EquipmentSlotDropTargetUI dropTarget)
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

    private void OnEquipmentSlotClicked(int index, InventorySlotModel data)
    {
        if (TryGetEquipmentAtIndex(index, out EquipmentInstance equipment))
        {
            SelectEquipment(equipment);
        }
    }

    private void OnEquipmentSlotRightClicked(int index, InventorySlotModel data)
    {
        if (!TryGetEquipmentAtIndex(index, out EquipmentInstance equipment))
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

        EquipmentInstance previewEquipment = GetPreviewEquipment();
        if (previewEquipment != null)
        {
            equipmentPreviewUI.ShowEquipment(previewEquipment);
        }
        else
        {
            equipmentPreviewUI.ShowEquipment();
        }
    }

    private EquipmentInstance GetPreviewEquipment()
    {
        return hoveredEquipment ?? selectedEquipment;
    }

    public void RefreshDebugData()
    {
        Load();
        RefreshVisuals();
    }

    private void SelectEquipment(EquipmentInstance equipment)
    {
        selectedEquipment = equipment;
        ClearHoveredEquipment();
        RefreshSelectionPresentation();
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
        RefreshVisuals();
        RefreshPlayerStatsPanel();
    }

    private void RefreshSelectionPresentation()
    {
        RefreshPreviewAndVisuals();
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
        if (!TryGetDisplayedEquipment(dropTarget, out EquipmentInstance equipment))
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
        if (!TryGetDisplayedEquipment(dropTarget, out EquipmentInstance equipment))
        {
            return;
        }

        ApplyHoveredEquipment(equipment);
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

    private bool TryGetEquipmentAtIndex(int index, out EquipmentInstance equipment)
    {
        equipment = GetEquipmentAtIndex(index);
        return equipment != null;
    }

    private EquipmentInstance GetDisplayedEquipment(EquipmentSlotDropTargetUI dropTarget)
    {
        return dropTarget != null ? dropTarget.DisplayedEquipment : null;
    }

    private bool TryGetDisplayedEquipment(EquipmentSlotDropTargetUI dropTarget, out EquipmentInstance equipment)
    {
        equipment = GetDisplayedEquipment(dropTarget);
        return equipment != null;
    }

    private void SetHoveredEquipment(EquipmentInstance equipment, int inventoryIndex = -1)
    {
        hoveredEquipment = equipment;
        hoveredEquipmentIndex = inventoryIndex;
    }

    private void ClearHoveredEquipment()
    {
        SetHoveredEquipment(null);
    }

    private void HandleEquipmentMutation(EquipmentInstance nextSelectedEquipment = null, bool rebuildLayout = true)
    {
        if (rebuildLayout)
        {
            RebuildInventoryLayout();
        }

        if (nextSelectedEquipment != null)
        {
            selectedEquipment = nextSelectedEquipment;
        }
        else if (selectedEquipment != null)
        {
            selectedEquipment = FindEquipmentById(selectedEquipment.InstanceId);
        }

        RefreshPresentation();
    }

    private void RefreshPreviewAndVisuals()
    {
        RefreshPreview();
        RefreshVisuals();
    }

    private void RefreshHoveredEquipmentFromCurrentIndex()
    {
        if (hoveredEquipmentIndex < 0 || hoveredEquipmentIndex >= equipmentInventoryLayout.Count)
        {
            ClearHoveredEquipment();
            RefreshPreview();
            return;
        }

        SetHoveredEquipment(GetEquipmentAtIndex(hoveredEquipmentIndex), hoveredEquipmentIndex);
        RefreshPreview();
    }
}
