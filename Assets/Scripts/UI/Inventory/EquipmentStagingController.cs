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
        ClearHoveredEquipment();

        if (equipmentPreviewUI != null)
        {
            equipmentPreviewUI.ShowEquipment(equipment);
        }

        RefreshEquippedSlotVisuals();
        RefreshGrid();
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

        if (!EquipmentInventoryInteractionService.ToggleEquipFromInventory(equipment, availableEquipment, equipmentDropTargets, dataFacade))
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

        SetHoveredEquipment(FindEquipmentById(equipmentInventoryLayout[index]), index);
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
        ClearHoveredEquipment();
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

        string equippedItemId = dataFacade.GetEquippedItemId(dropTarget.LoadoutSlotId);
        if (string.IsNullOrWhiteSpace(equippedItemId))
        {
            return;
        }

        if (!EquipmentInventoryInteractionService.UnequipFromLoadoutSlot(dropTarget.LoadoutSlotId, dataFacade))
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

        SetHoveredEquipment(dropTarget.DisplayedEquipment);
        RefreshPreview();
        RefreshEquippedSlotVisuals();
        RefreshGrid();
    }

    private void HandleEquippedSlotHoverExited(EquipmentSlotDropTargetUI dropTarget)
    {
        ClearHoveredEquipment();
        RefreshPreview();
        RefreshEquippedSlotVisuals();
        RefreshGrid();
    }

    private void HandleEquipmentDropped(EquipmentSlotDropTargetUI dropTarget, DragItemPayload payload)
    {
        if (!EquipmentInventoryInteractionService.HandleDropToEquipmentSlot(dropTarget, payload, availableEquipment, dataFacade))
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
        return EquipmentInventoryInteractionService.CanAcceptInventoryDrop(index, payload, equipmentInventoryLayout, dataFacade);
    }

    private void HandleEquipmentInventorySlotDrop(int targetIndex, DragItemPayload payload)
    {
        if (!EquipmentInventoryInteractionService.HandleInventorySlotDrop(targetIndex, payload, equipmentInventoryLayout, availableEquipment, dataFacade))
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
            dataFacade.GetEquippedItemId);
        equipmentInventoryLayout.Clear();
        equipmentInventoryLayout.AddRange(rebuiltLayout);
    }

    private EquipmentInstance FindEquipmentById(string equipmentId)
    {
        if (string.IsNullOrWhiteSpace(equipmentId))
        {
            return null;
        }

        return EquipmentInstanceLookup.FindById(availableEquipment, equipmentId);
    }

    private void RefreshHoveredEquipmentFromCurrentIndex()
    {
        if (hoveredEquipmentIndex < 0 || hoveredEquipmentIndex >= equipmentInventoryLayout.Count)
        {
            ClearHoveredEquipment();
            RefreshPreview();
            return;
        }

        hoveredEquipment = FindEquipmentById(equipmentInventoryLayout[hoveredEquipmentIndex]);
        RefreshPreview();
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
}
