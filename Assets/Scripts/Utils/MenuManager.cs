using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void NewGame()
    {
        RunData.SelectedWeapon = null;
        RunData.SelectedMap = null;
        SceneManager.LoadScene("Staging");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
