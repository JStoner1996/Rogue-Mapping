using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : SingletonBehaviour<UIController>
{
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider playerExperienceSlider;
    [SerializeField] private TMP_Text experienceText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text killText;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private RunCompletePanelUI runCompletePanel;

    [Header("Level Up")]
    [SerializeField] private Transform levelUpButtonParent;
    [SerializeField] private LevelUpButton levelUpButtonPrefab;
    [SerializeField] private int levelUpButtonCount = 3;

    private LevelUpButton[] levelUpButtons = System.Array.Empty<LevelUpButton>();

    private PlayerHealth playerHealth;
    private PlayerExperience playerExperience;

    public GameObject GameOverPanel => gameOverPanel;
    public GameObject PausePanel => pausePanel;
    public GameObject LevelUpPanel => levelUpPanel;

    private void Awake()
    {
        if (!TryInitializeSingleton())
        {
            return;
        }
    }

    private void Start()
    {
        CachePlayerReferences();
        BuildLevelUpButtons();
    }

    public void UpdateHealthSlider()
    {
        CachePlayerReferences();

        if (playerHealth == null)
        {
            return;
        }

        UpdateSlider(playerHealthSlider, healthText, playerHealth.MaxHealth, playerHealth.CurrentHealth);
    }

    public void UpdateExperienceSlider()
    {
        CachePlayerReferences();
        if (playerExperience == null || playerExperience.LevelThresholds.Count == 0)
        {
            return;
        }

        int thresholdIndex = Mathf.Clamp(playerExperience.CurrentLevel - 1, 0, playerExperience.LevelThresholds.Count - 1);
        UpdateSlider(
            playerExperienceSlider,
            experienceText,
            playerExperience.LevelThresholds[thresholdIndex],
            playerExperience.CurrentExperience);
    }

    public void UpdateTimer(float timer) => UpdateVictoryConditionUI(timer);

    private string FormatTime(float timer)
    {
        int min = Mathf.FloorToInt(timer / 60f);
        int sec = Mathf.FloorToInt(timer % 60f);
        return min + ":" + sec.ToString("00");
    }

    public void LevelUpPanelOpen()
    {
        BuildLevelUpButtons();
        SetPanelState(levelUpPanel, true, 0f);
    }

    public void LevelUpPanelClosed()
    {
        SetPanelState(levelUpPanel, false, 1f);
    }

    public void ShowRunCompletePanel(MapInstance completedMap)
    {
        runCompletePanel?.Show(completedMap);
        Time.timeScale = 0f;
    }

    public void HideRunCompletePanel()
    {
        runCompletePanel?.Hide();
        Time.timeScale = 1f;
    }

    public void UpdateVictoryConditionUI(float elapsedTime)
    {
        (string timer, string counter) = BuildVictoryDisplayTexts(elapsedTime);
        bool showTimer = !string.IsNullOrEmpty(timer);
        bool showCounter = !string.IsNullOrEmpty(counter);

        if (timerText != null)
        {
            timerText.enabled = showTimer;
            if (showTimer) timerText.text = timer;
        }

        if (killText != null)
        {
            killText.enabled = showCounter;
            if (showCounter) killText.text = counter;
        }
    }

    // The HUD has only two text slots, so this resolves whichever victory state should occupy each one.
    private (string timer, string counter) BuildVictoryDisplayTexts(float elapsedTime)
    {
        MapInstance selectedMap = RunData.SelectedMap;
        int enemyKills = GameManager.Instance != null ? GameManager.Instance.EnemyKills : 0;

        if (selectedMap == null)
        {
            return (FormatTime(elapsedTime), enemyKills.ToString());
        }

        return selectedMap.VictoryConditionType switch
        {
            VictoryConditionType.Time => ($"{FormatTime(elapsedTime)} / {FormatTime(selectedMap.VictoryTarget * 60f)}", null),
            VictoryConditionType.Kills => (null, $"{enemyKills} / {selectedMap.VictoryTarget}"),
            _ => (FormatTime(elapsedTime), null)
        };
    }

    private void CachePlayerReferences()
    {
        if (PlayerController.Instance == null)
        {
            return;
        }

        playerHealth ??= PlayerController.Instance.PlayerHealthComponent;
        playerExperience ??= PlayerController.Instance.PlayerExperienceComponent;
    }

    public LevelUpButton[] GetLevelUpButtons()
    {
        if (levelUpButtons == null || levelUpButtons.Length == 0)
        {
            BuildLevelUpButtons();
        }

        return levelUpButtons;
    }

    private void BuildLevelUpButtons()
    {
        if (levelUpButtonParent == null || levelUpButtonPrefab == null)
        {
            return;
        }

        int buttonCount = Mathf.Max(1, levelUpButtonCount);
        if (!NeedsButtonRebuild(buttonCount))
        {
            return;
        }

        foreach (Transform child in levelUpButtonParent)
        {
            Destroy(child.gameObject);
        }

        levelUpButtons = new LevelUpButton[buttonCount];

        for (int i = 0; i < buttonCount; i++)
        {
            levelUpButtons[i] = Instantiate(levelUpButtonPrefab, levelUpButtonParent);
        }
    }

    private static void UpdateSlider(Slider slider, TMP_Text text, float maxValue, float value)
    {
        if (slider == null || text == null)
        {
            return;
        }

        slider.maxValue = maxValue;
        slider.value = value;
        text.text = $"{Mathf.RoundToInt(value)} / {Mathf.RoundToInt(maxValue)}";
    }

    private static void SetPanelState(GameObject panel, bool isVisible, float timeScale)
    {
        if (panel != null)
        {
            panel.SetActive(isVisible);
        }

        Time.timeScale = timeScale;
    }

    private bool NeedsButtonRebuild(int buttonCount)
    {
        if (levelUpButtons == null || levelUpButtons.Length != buttonCount)
        {
            return true;
        }

        for (int i = 0; i < levelUpButtons.Length; i++)
        {
            if (levelUpButtons[i] == null)
            {
                return true;
            }
        }

        return false;
    }
}
