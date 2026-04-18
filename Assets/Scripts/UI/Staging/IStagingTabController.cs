// Shared surface for staging tab controllers that can refresh their grid and preview.
public interface IStagingTabController
{
    void RefreshGrid();
    void RefreshPreview();
}
