using System.Collections.Generic;

// Coordinates the maps tab state, selection, and preview updates.
public class MapStagingController : SimpleListStagingController<MapInstance>
{
    private readonly IMapDataFacade dataFacade;

    public MapStagingController(InventoryGridUI mapGrid, ItemDetailsPanelUI mapPreviewUI, IMapDataFacade dataFacade = null)
        : base(mapGrid, mapPreviewUI != null ? new System.Action<MapInstance>(mapPreviewUI.ShowMap) : null)
    {
        this.dataFacade = dataFacade ?? new MetaProgressionMapDataFacade();
    }

    public MapInstance SelectedMap => SelectedItem;

    public void LoadStarterMaps(int starterCount, VictoryConditionType defaultVictoryCondition, int defaultVictoryTarget)
    {
        dataFacade.EnsureStarterMaps(starterCount, defaultVictoryCondition, defaultVictoryTarget);
        List<MapInstance> availableMaps = dataFacade.GetOwnedMaps();
        SetItems(availableMaps);
        SelectedItem = availableMaps.Count > 0 ? availableMaps[0] : null;
        ClearHoveredItem();
    }

    protected override string GetItemId(MapInstance item) => item.BaseMapId + "|" + item.DisplayName;
    protected override string GetItemLabel(MapInstance item) => item.DisplayName;
    protected override UnityEngine.Sprite GetItemIcon(MapInstance item) => item.Icon;
    protected override void OnSelectionChanged(MapInstance item) => RunData.SelectedMap = item;
}
