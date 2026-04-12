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
    public int enemyKills { get; private set; }

    private bool runCompleted;

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
        MetaProgressionService.EnsureLoaded();
        RunData.GetSelectedMapOrDefault();
        gameActive = true;
        enemyKills = 0;
        runCompleted = false;
    }

    void OnEnable()
    {
        Enemy.EnemyKilled += OnEnemyKilled;
    }

    void OnDisable()
    {
        Enemy.EnemyKilled -= OnEnemyKilled;
    }

    void Update()
    {
        if (!gameActive)
        {
            return;
        }

        gameTime += Time.deltaTime;
        UIController.Instance.UpdateTimer(gameTime);
        CheckVictoryCondition();

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

    public void CompleteRun()
    {
        if (runCompleted)
        {
            return;
        }

        runCompleted = true;
        gameActive = false;

        bool grantedAtlasPoint = false;

        if (RunData.SelectedMap != null)
        {
            grantedAtlasPoint = MapProgressionData.MarkCompleted(RunData.SelectedMap.BaseMapId);
        }

        Debug.Log(grantedAtlasPoint
            ? $"Completed {RunData.SelectedMap?.BaseMapName}. Atlas point awarded."
            : $"Completed {RunData.SelectedMap?.BaseMapName}. No atlas point awarded.");

        StartCoroutine(ReturnToStagingAfterCompletion());
    }

    IEnumerator ShowGameOverScreen()
    {
        yield return new WaitForSeconds(0.5f);
        UIController.Instance.gameOverPanel.SetActive(true);
        AudioManager.Instance.Play(SoundType.GameOver);
    }

    IEnumerator ReturnToStagingAfterCompletion()
    {
        yield return new WaitForSeconds(0.5f);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Staging");
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

    private void OnEnemyKilled()
    {
        enemyKills++;
        CheckVictoryCondition();
    }

    private void CheckVictoryCondition()
    {
        if (!gameActive || runCompleted || RunData.SelectedMap == null)
        {
            return;
        }

        switch (RunData.SelectedMap.VictoryConditionType)
        {
            case VictoryConditionType.Time:
                if (gameTime >= RunData.SelectedMap.VictoryTarget * 60f)
                {
                    CompleteRun();
                }
                break;

            case VictoryConditionType.Kills:
                if (enemyKills >= RunData.SelectedMap.VictoryTarget)
                {
                    CompleteRun();
                }
                break;
        }
    }
}
