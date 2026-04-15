using System.Collections.Generic;

// Coordinates the maps tab state, selection, and preview updates.
public class MapStagingController : IStagingTabController
{
    private readonly InventoryGridUI mapGrid;
    private readonly ItemDetailsPanelUI mapPreviewUI;

    private readonly List<MapInstance> availableMaps = new List<MapInstance>();
    private MapInstance selectedMap;
    private MapInstance hoveredMap;

    public MapStagingController(InventoryGridUI mapGrid, ItemDetailsPanelUI mapPreviewUI)
    {
        this.mapGrid = mapGrid;
        this.mapPreviewUI = mapPreviewUI;
    }

    public MapInstance SelectedMap => selectedMap;

    public void LoadStarterMaps(int starterCount, VictoryConditionType defaultVictoryCondition, int defaultVictoryTarget)
    {
        MetaProgressionService.EnsureStarterMaps(starterCount, defaultVictoryCondition, defaultVictoryTarget);
        availableMaps.Clear();
        availableMaps.AddRange(MetaProgressionService.GetOwnedMaps());
        selectedMap = availableMaps.Count > 0 ? availableMaps[0] : null;
        hoveredMap = null;
    }

    public void SetSelectedMap(MapInstance map)
    {
        selectedMap = map;
        hoveredMap = null;
        RunData.SelectedMap = map;
        mapPreviewUI?.ShowMap(map);
        RefreshGrid();
    }

    public void RefreshGrid()
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

    public void RefreshPreview()
    {
        mapPreviewUI?.ShowMap(hoveredMap ?? selectedMap);
    }

    private void OnMapSlotClicked(int index, InventorySlotModel data)
    {
        if (index < 0 || index >= availableMaps.Count)
        {
            return;
        }

        SetSelectedMap(availableMaps[index]);
    }

    private void OnMapSlotHoverEnter(int index, InventorySlotModel data)
    {
        if (index < 0 || index >= availableMaps.Count)
        {
            return;
        }

        hoveredMap = availableMaps[index];
        mapPreviewUI?.ShowMap(hoveredMap);
        RefreshGrid();
    }

    private void OnMapSlotHoverExit(int index, InventorySlotModel data)
    {
        hoveredMap = null;
        RefreshPreview();
        RefreshGrid();
    }
}
