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
        PlayerHealth health = PlayerController.Instance.GetComponent<PlayerHealth>();

        playerHealthSlider.maxValue = health.MaxHealth;
        playerHealthSlider.value = health.CurrentHealth;
        healthText.text = $"{playerHealthSlider.value} / {playerHealthSlider.maxValue}";
    }

    public void UpdateExperienceSlider()
    {
        PlayerExperience experience = PlayerController.Instance.GetComponent<PlayerExperience>();

        playerExperienceSlider.maxValue = experience.LevelThresholds[experience.CurrentLevel - 1];
        playerExperienceSlider.value = experience.CurrentExperience;
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
