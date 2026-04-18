using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void NewGame()
    {
        MetaProgressionService.EnsureLoaded();
        RunData.ClearSelections();
        SceneManager.LoadScene(SceneCatalog.Staging);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
