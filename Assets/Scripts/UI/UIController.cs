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


    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject levelUpPanel;

    public LevelUpButton[] levelUpButtons;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void UpdateHealthSlider()
    {
        PlayerController player = PlayerController.Instance;

        playerHealthSlider.maxValue = player.playerMaxHealth;
        playerHealthSlider.value = player.playerHealth;
        healthText.text = $"{playerHealthSlider.value} / {playerHealthSlider.maxValue}";
    }

    public void UpdateExperienceSlider()
    {
        PlayerController player = PlayerController.Instance;

        playerExperienceSlider.maxValue = player.playerLevels[player.currentLevel - 1];
        playerExperienceSlider.value = player.experience;
        experienceText.text = $"{playerExperienceSlider.value} / {playerExperienceSlider.maxValue}";
    }

    public void UpdateTimer(float timer)
    {
        float min = Mathf.FloorToInt(timer / 60f);
        float sec = Mathf.FloorToInt(timer % 60f);

        timerText.text = min + ":" + sec.ToString("00");
    }

    public void LevelUpPanelOpen()
    {
        levelUpPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void LevelUpPanelClosed()
    {
        levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
