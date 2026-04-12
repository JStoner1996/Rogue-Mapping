using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    private float baseMaxHealth;
    private float maxHealthMultiplier;
    private float immunityDuration;
    private bool initialized;

    private bool isImmune;
    private float immunityTimer;

    public float MaxHealth => baseMaxHealth * (1f + maxHealthMultiplier);
    public float CurrentHealth { get; private set; }

    public void Configure(float configuredMaxHealth, float configuredImmunityDuration)
    {
        baseMaxHealth = configuredMaxHealth;
        maxHealthMultiplier = 0f;
        immunityDuration = configuredImmunityDuration;
        CurrentHealth = MaxHealth;
        initialized = true;
    }

    public void ApplyMaxHealthModifier(float value)
    {
        float previousMaxHealth = MaxHealth;
        maxHealthMultiplier += value;
        float newMaxHealth = MaxHealth;
        CurrentHealth = Mathf.Min(CurrentHealth + (newMaxHealth - previousMaxHealth), newMaxHealth);
        UIController.Instance.UpdateHealthSlider();
    }

    void Start()
    {
        if (!initialized)
        {
            return;
        }

        UIController.Instance.UpdateHealthSlider();
    }

    void Update()
    {
        if (immunityTimer > 0f)
        {
            immunityTimer -= Time.deltaTime;
            return;
        }

        isImmune = false;
    }

    void OnEnable()
    {
        HealthPickup.onHealthPickup += GainHealth;
    }

    void OnDisable()
    {
        HealthPickup.onHealthPickup -= GainHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isImmune)
        {
            return;
        }

        isImmune = true;
        immunityTimer = immunityDuration;
        CurrentHealth -= damage;

        UIController.Instance.UpdateHealthSlider();

        if (CurrentHealth > 0f)
        {
            return;
        }

        gameObject.SetActive(false);
        GameManager.Instance.GameOver();
    }

    public void GainHealth(int healthAmount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + healthAmount, MaxHealth);
        UIController.Instance.UpdateHealthSlider();
    }
}
