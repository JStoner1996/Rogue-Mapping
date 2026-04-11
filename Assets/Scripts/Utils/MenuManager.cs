using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void NewGame()
    {
        RunData.SelectedWeapon = null;
        RunData.SelectedMap = null;
        SceneManager.LoadScene("Weapon Select");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
