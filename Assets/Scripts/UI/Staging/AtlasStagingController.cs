// Coordinates the atlas staging tab and refreshes the atlas UI when it becomes active.
public class AtlasStagingController : IStagingTabController
{
    private readonly AtlasScreenUI atlasScreenUI;

    public AtlasStagingController(AtlasScreenUI atlasScreenUI)
    {
        this.atlasScreenUI = atlasScreenUI;
    }

    public void RefreshGrid()
    {
        atlasScreenUI?.Refresh();
    }

    public void RefreshPreview()
    {
        atlasScreenUI?.Refresh();
    }
}
