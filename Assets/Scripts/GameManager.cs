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
    public bool victoryConditionMet { get; private set; }
    public bool finalBossEncounterActive => remainingFinalBosses > 0;

    private bool runCompleted;
    private EnemySpawner enemySpawner;
    private int remainingFinalBosses;

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
        RunLootService.Clear();
        RunData.GetSelectedMapOrDefault();
        gameActive = true;
        enemyKills = 0;
        runCompleted = false;
        victoryConditionMet = false;
        remainingFinalBosses = 0;
        enemySpawner = FindAnyObjectByType<EnemySpawner>();
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
        RunLootService.Clear();
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
        UIController.Instance.ShowRunCompletePanel(RunData.SelectedMap);
    }

    IEnumerator ShowGameOverScreen()
    {
        yield return new WaitForSeconds(0.5f);
        UIController.Instance.gameOverPanel.SetActive(true);
        AudioManager.Instance.Play(SoundType.GameOver);
    }

    public void FinalizeCompletedRun()
    {
        Time.timeScale = 1f;
        RunData.SelectedMap = null;
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

    private void OnEnemyKilled(Enemy enemy)
    {
        enemyKills++;

        if (enemy != null && enemy.Archetype == EnemyArchetype.Boss && remainingFinalBosses > 0)
        {
            remainingFinalBosses--;

            if (remainingFinalBosses <= 0)
            {
                CompleteRun();
                return;
            }
        }

        CheckVictoryCondition();
    }

    private void CheckVictoryCondition()
    {
        if (!gameActive || runCompleted || victoryConditionMet || RunData.SelectedMap == null)
        {
            return;
        }

        switch (RunData.SelectedMap.VictoryConditionType)
        {
            case VictoryConditionType.Time:
                if (gameTime >= RunData.SelectedMap.VictoryTarget * 60f)
                {
                    BeginFinalBossEncounter();
                }
                break;

            case VictoryConditionType.Kills:
                if (enemyKills >= RunData.SelectedMap.VictoryTarget)
                {
                    BeginFinalBossEncounter();
                }
                break;
        }
    }

    private void BeginFinalBossEncounter()
    {
        if (victoryConditionMet || runCompleted)
        {
            return;
        }

        int bossCount = GetFinalBossCount();

        if (enemySpawner == null)
        {
            enemySpawner = FindAnyObjectByType<EnemySpawner>();
        }

        if (enemySpawner == null || !enemySpawner.SpawnFinalBosses(bossCount))
        {
            Debug.LogError("Final boss encounter could not start because no boss spawn entry is available.");
            return;
        }

        victoryConditionMet = true;
        remainingFinalBosses = bossCount;
    }

    private int GetFinalBossCount()
    {
        return 1;
    }
}
