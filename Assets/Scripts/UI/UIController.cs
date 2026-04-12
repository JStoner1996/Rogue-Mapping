using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider playerExperienceSlider;
    [SerializeField] private TMP_Text experienceText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text killText;


    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject levelUpPanel;

    [Header("Level Up")]
    [SerializeField] private Transform levelUpButtonParent;
    [SerializeField] private LevelUpButton levelUpButtonPrefab;
    [SerializeField] private int levelUpButtonCount = 3;

    private LevelUpButton[] levelUpButtons = System.Array.Empty<LevelUpButton>();

    private PlayerHealth playerHealth;
    private PlayerExperience playerExperience;

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
        CachePlayerReferences();
        BuildLevelUpButtons();
    }

    public void UpdateHealthSlider()
    {
        if (playerHealth == null)
        {
            CachePlayerReferences();
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

        playerExperienceSlider.maxValue = playerExperience.LevelThresholds[playerExperience.CurrentLevel - 1];
        playerExperienceSlider.value = playerExperience.CurrentExperience;
        experienceText.text = $"{playerExperienceSlider.value} / {playerExperienceSlider.maxValue}";
    }

    public void UpdateTimer(float timer)
    {
        if (RunData.SelectedMap == null)
        {
            timerText.text = FormatTime(timer);

            if (killText != null)
            {
                killText.text = GameManager.Instance != null ? GameManager.Instance.enemyKills.ToString() : "0";
            }

            return;
        }

        if (RunData.SelectedMap.VictoryConditionType == VictoryConditionType.Time)
        {
            timerText.text = $"{FormatTime(timer)} / {FormatTime(RunData.SelectedMap.VictoryTarget * 60f)}";

            if (killText != null && GameManager.Instance != null)
            {
                killText.text = GameManager.Instance.enemyKills.ToString();
            }

            return;
        }

        timerText.text = FormatTime(timer);

        if (killText != null && GameManager.Instance != null)
        {
            killText.text = $"{GameManager.Instance.enemyKills} / {RunData.SelectedMap.VictoryTarget}";
        }
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
        levelUpPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void LevelUpPanelClosed()
    {
        levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void CachePlayerReferences()
    {
        if (PlayerController.Instance == null)
        {
            return;
        }

        playerHealth = PlayerController.Instance.GetComponent<PlayerHealth>();
        playerExperience = PlayerController.Instance.GetComponent<PlayerExperience>();
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
        bool needsRebuild = levelUpButtons == null || levelUpButtons.Length != buttonCount;

        if (!needsRebuild)
        {
            for (int i = 0; i < levelUpButtons.Length; i++)
            {
                if (levelUpButtons[i] == null)
                {
                    needsRebuild = true;
                    break;
                }
            }
        }

        if (!needsRebuild)
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
}
