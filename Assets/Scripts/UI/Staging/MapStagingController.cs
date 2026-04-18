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

    public void Load(VictoryConditionType defaultVictoryCondition, int defaultVictoryTarget)
    {
        dataFacade.EnsureStarterMaps(defaultVictoryCondition, defaultVictoryTarget);
        LoadItems(dataFacade.GetOwnedMaps());
    }

    protected override string GetItemId(MapInstance item) => item.BaseMapId + "|" + item.DisplayName;
    protected override string GetItemLabel(MapInstance item) => item.DisplayName;
    protected override UnityEngine.Sprite GetItemIcon(MapInstance item) => item.Icon;
    protected override void OnSelectionChanged(MapInstance item) => RunData.SelectedMap = item;
}
