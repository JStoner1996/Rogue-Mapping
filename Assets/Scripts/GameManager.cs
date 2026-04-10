using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public InputActionReference pause;

    public float gameTime;
    public bool gameActive;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        gameActive = true;
    }

    void Update()
    {
        if (!gameActive)
        {
            return;
        }

        gameTime += Time.deltaTime;
        UIController.Instance.UpdateTimer(gameTime);

        if (pause.action.WasPressedThisFrame())
        {
            Pause();
        }
    }

    public void GameOver()
    {
        gameActive = false;
        StartCoroutine(ShowGameOverScreen());
    }

    IEnumerator ShowGameOverScreen()
    {
        yield return new WaitForSeconds(0.5f);
        UIController.Instance.gameOverPanel.SetActive(true);
        AudioManager.Instance.Play(SoundType.GameOver);
    }

    public void Restart()
    {
        SceneManager.LoadScene("Game");
    }

    public void Pause()
    {
        if (UIController.Instance.gameOverPanel.activeSelf)
        {
            return;
        }

        bool isPaused = UIController.Instance.pausePanel.activeSelf;
        UIController.Instance.pausePanel.SetActive(!isPaused);
        Time.timeScale = isPaused ? 1f : 0f;

        if (isPaused)
        {
            AudioManager.Instance.Play(SoundType.Unpause);
            return;
        }

        AudioManager.Instance.Play(SoundType.Pause);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
        Time.timeScale = 1f;
    }
}
