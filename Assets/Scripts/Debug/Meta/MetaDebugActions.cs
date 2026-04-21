using UnityEngine;
using UnityEngine.SceneManagement;

public class MetaDebugActions : MonoBehaviour
{
    [Header("Default Map Rebuild")]
    [SerializeField] private VictoryConditionType defaultMapVictoryCondition = VictoryConditionType.Kills;
    [SerializeField] private int defaultMapVictoryTarget = 10;
    [Header("Debug Panels")]
    [SerializeField] private EquipmentDebugGenerationPanelUI equipmentDebugGenerationPanel;

    public void ResetAllMetaProgression()
    {
        MetaProgressionService.ResetAllProgress();
        RebuildStarterMaps();
        Debug.Log("Reset all meta progression and rebuilt starter maps.");
    }

    public void ResetMapProgression()
    {
        MetaProgressionService.ResetMapProgression();
        Debug.Log("Reset map completion and atlas points.");
    }

    public void ClearMapInventory()
    {
        MetaProgressionService.ClearMapInventory();
        RebuildStarterMaps();
        Debug.Log("Cleared map inventory and rebuilt starter maps.");
    }

    public void ClearEquipmentInventory()
    {
        MetaProgressionService.ClearEquipmentInventory();
        Debug.Log("Cleared equipment inventory.");
    }

    public void AddAtlasPoint()
    {
        MetaProgressionService.AddAtlasPoint();
        Debug.Log("Atlas Point Added.");
    }

    public void ClearEquipmentLoadout()
    {
        MetaProgressionService.ClearEquipmentLoadout();
        Debug.Log("Cleared equipment loadout.");
    }

    public void RebuildStarterMaps()
    {
        MetaProgressionService.EnsureStarterMaps(
            defaultMapVictoryCondition,
            defaultMapVictoryTarget);
    }

    public void ReloadStagingScene()
    {
        SceneManager.LoadScene(SceneCatalog.Staging);
    }

    public void OpenEquipmentGeneratorPanel()
    {
        if (equipmentDebugGenerationPanel != null)
        {
            equipmentDebugGenerationPanel.Show();
        }
    }
}
