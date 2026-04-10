using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void NewGame()
    {
        SceneManager.LoadScene("Weapon Select");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
