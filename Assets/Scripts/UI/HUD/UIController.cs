using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : SingletonBehaviour<UIController>
{
    private enum VictoryDisplayMode
    {
        Timer,
        Counter
    }

    private struct VictoryDisplayData
    {
        public VictoryDisplayMode DisplayMode;
        public string PrimaryText;
        public string SecondaryText;

        public VictoryDisplayData(VictoryDisplayMode displayMode, string primaryText, string secondaryText = "")
        {
            DisplayMode = displayMode;
            PrimaryText = primaryText;
            SecondaryText = secondaryText;
        }
    }

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
        if (playerHealth == null)
        {
            CachePlayerReferences();
        }

        if (playerHealth == null || playerHealthSlider == null || healthText == null)
        {
            return;
        }

        playerHealthSlider.maxValue = playerHealth.MaxHealth;
        playerHealthSlider.value = playerHealth.CurrentHealth;
        int currentHealth = Mathf.RoundToInt(playerHealth.CurrentHealth);
        int maxHealth = Mathf.RoundToInt(playerHealth.MaxHealth);
        healthText.text = $"{currentHealth} / {maxHealth}";
    }

    public void UpdateExperienceSlider()
    {
        if (playerExperience == null)
        {
            CachePlayerReferences();
        }

        if (playerExperience == null
            || playerExperience.LevelThresholds.Count == 0
            || playerExperienceSlider == null
            || experienceText == null)
        {
            return;
        }

        int thresholdIndex = Mathf.Clamp(playerExperience.CurrentLevel - 1, 0, playerExperience.LevelThresholds.Count - 1);
        playerExperienceSlider.maxValue = playerExperience.LevelThresholds[thresholdIndex];
        playerExperienceSlider.value = playerExperience.CurrentExperience;
        experienceText.text = $"{playerExperienceSlider.value} / {playerExperienceSlider.maxValue}";
    }

    public void UpdateTimer(float timer)
    {
        ApplyVictoryDisplay(BuildVictoryDisplayData(timer));
    }

    private string FormatTime(float timer)
    {
        int min = Mathf.FloorToInt(timer / 60f);
        int sec = Mathf.FloorToInt(timer % 60f);
        return min + ":" + sec.ToString("00");
    }

    public void LevelUpPanelOpen()
    {
        BuildLevelUpButtons();

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void LevelUpPanelClosed()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        Time.timeScale = 1f;
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
        VictoryDisplayData displayData = BuildVictoryDisplayData(elapsedTime);
        ApplyVictoryDisplay(displayData);
    }

    private VictoryDisplayData BuildVictoryDisplayData(float elapsedTime)
    {
        MapInstance selectedMap = RunData.SelectedMap;
        int enemyKills = GameManager.Instance != null ? GameManager.Instance.EnemyKills : 0;

        if (selectedMap == null)
        {
            return new VictoryDisplayData(
                VictoryDisplayMode.Timer,
                FormatTime(elapsedTime),
                enemyKills.ToString());
        }

        return selectedMap.VictoryConditionType switch
        {
            VictoryConditionType.Time => new VictoryDisplayData(
                VictoryDisplayMode.Timer,
                $"{FormatTime(elapsedTime)} / {FormatTime(selectedMap.VictoryTarget * 60f)}"),

            VictoryConditionType.Kills => new VictoryDisplayData(
                VictoryDisplayMode.Counter,
                $"{enemyKills} / {selectedMap.VictoryTarget}"),

            _ => new VictoryDisplayData(
                VictoryDisplayMode.Timer,
                FormatTime(elapsedTime))
        };
    }

    private void ApplyVictoryDisplay(VictoryDisplayData displayData)
    {
        bool showTimer = displayData.DisplayMode == VictoryDisplayMode.Timer;
        bool showCounter = displayData.DisplayMode == VictoryDisplayMode.Counter;

        // The HUD reuses the counter label for both kill goals and the "secondary" display when time is primary.
        if (timerText != null)
        {
            timerText.enabled = showTimer;

            if (showTimer)
            {
                timerText.text = displayData.PrimaryText;
            }
        }

        if (killText != null)
        {
            bool shouldShowSecondaryCounter = showCounter || !string.IsNullOrEmpty(displayData.SecondaryText);
            killText.enabled = shouldShowSecondaryCounter;

            if (showCounter)
            {
                killText.text = displayData.PrimaryText;
            }
            else if (shouldShowSecondaryCounter)
            {
                killText.text = displayData.SecondaryText;
            }
        }
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
