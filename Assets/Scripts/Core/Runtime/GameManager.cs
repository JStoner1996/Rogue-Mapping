using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : SingletonBehaviour<GameManager>
{
    [SerializeField] private InputActionReference pause;

    public float GameTime { get; private set; }
    public bool IsGameActive { get; private set; }
    public int EnemyKills { get; private set; }
    public bool VictoryConditionMet { get; private set; }
    public bool IsFinalBossEncounterActive => remainingFinalBosses > 0;

    private bool runCompleted;
    private EnemySpawner enemySpawner;
    private int remainingFinalBosses;

    private void Awake()
    {
        if (!TryInitializeSingleton())
        {
            return;
        }
    }

    private void Start()
    {
        InitializeRunState();
    }

    private void OnEnable()
    {
        Enemy.EnemyKilled += OnEnemyKilled;
    }

    private void OnDisable()
    {
        Enemy.EnemyKilled -= OnEnemyKilled;
    }

    private void Update()
    {
        if (!IsGameActive)
        {
            return;
        }

        GameTime += Time.deltaTime;
        UIController.Instance?.UpdateTimer(GameTime);
        CheckVictoryCondition();

        if (pause != null && pause.action != null && pause.action.WasPressedThisFrame())
        {
            Pause();
        }
    }

    public void GameOver()
    {
        if (!IsGameActive)
        {
            return;
        }

        IsGameActive = false;
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
        IsGameActive = false;

        bool grantedAtlasPoint = RunData.SelectedMap != null
            && MapProgressionData.MarkCompleted(RunData.SelectedMap.BaseMapId);

        Debug.Log(grantedAtlasPoint
            ? $"Completed {RunData.SelectedMap?.BaseMapName}. Atlas point awarded."
            : $"Completed {RunData.SelectedMap?.BaseMapName}. No atlas point awarded.");
        UIController.Instance?.ShowRunCompletePanel(RunData.SelectedMap);
    }

    public void FinalizeCompletedRun()
    {
        Time.timeScale = 1f;
        RunData.SelectedMap = null;
        SceneManager.LoadScene(SceneCatalog.Staging);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneCatalog.Game);
    }

    public void Pause()
    {
        UIController uiController = UIController.Instance;

        if (uiController == null || (uiController.GameOverPanel != null && uiController.GameOverPanel.activeSelf))
        {
            return;
        }

        bool isPaused = uiController.PausePanel != null && uiController.PausePanel.activeSelf;

        SetPanelPausedState(uiController.PausePanel, !isPaused);
        AudioManager.Instance?.Play(isPaused ? SoundType.Unpause : SoundType.Pause);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneCatalog.MainMenu);
    }

    private void InitializeRunState()
    {
        MetaProgressionService.EnsureLoaded();
        RunLootService.Clear();
        RunData.GetSelectedMapOrDefault();

        GameTime = 0f;
        IsGameActive = true;
        EnemyKills = 0;
        runCompleted = false;
        VictoryConditionMet = false;
        remainingFinalBosses = 0;
        enemySpawner = FindAnyObjectByType<EnemySpawner>();
    }

    private IEnumerator ShowGameOverScreen()
    {
        yield return new WaitForSeconds(0.5f);
        SetPanelPausedState(UIController.Instance?.GameOverPanel, true);
        AudioManager.Instance?.Play(SoundType.GameOver);
    }

    private void OnEnemyKilled(Enemy enemy)
    {
        EnemyKills++;

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
        MapInstance selectedMap = RunData.SelectedMap;

        if (!IsGameActive || runCompleted || VictoryConditionMet || selectedMap == null)
        {
            return;
        }

        switch (selectedMap.VictoryConditionType)
        {
            case VictoryConditionType.Time:
                if (GameTime >= selectedMap.VictoryTarget * 60f)
                {
                    BeginFinalBossEncounter();
                }
                break;

            case VictoryConditionType.Kills:
                if (EnemyKills >= selectedMap.VictoryTarget)
                {
                    BeginFinalBossEncounter();
                }
                break;
        }
    }

    private void BeginFinalBossEncounter()
    {
        if (VictoryConditionMet || runCompleted)
        {
            return;
        }

        int bossCount = GetFinalBossCount();

        // The run is only complete after the encounter-spawned bosses are defeated, not when the threshold is reached.
        if (!TryGetEnemySpawner(out EnemySpawner spawner) || !spawner.SpawnFinalBosses(bossCount))
        {
            Debug.LogError("Final boss encounter could not start because no boss spawn entry is available.");
            return;
        }

        VictoryConditionMet = true;
        remainingFinalBosses = bossCount;
    }

    private bool TryGetEnemySpawner(out EnemySpawner spawner)
    {
        if (enemySpawner == null)
        {
            enemySpawner = FindAnyObjectByType<EnemySpawner>();
        }

        spawner = enemySpawner;
        return spawner != null;
    }

    private int GetFinalBossCount() => 1;

    private static void SetPanelPausedState(GameObject panel, bool isVisible)
    {
        if (panel != null)
        {
            panel.SetActive(isVisible);
        }

        Time.timeScale = isVisible ? 0f : 1f;
    }
}
