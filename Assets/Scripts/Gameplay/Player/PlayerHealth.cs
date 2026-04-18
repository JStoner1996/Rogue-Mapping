using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    private const float ArmorMitigationDenominator = 100f;

    private float baseMaxHealth;
    private float flatMaxHealthBonus;
    private float maxHealthMultiplier;
    private float immunityDuration;
    private bool initialized;
    private float armor;
    private float healthRegenPerSecond;

    private bool isImmune;
    private float immunityTimer;

    public float MaxHealth => (baseMaxHealth + flatMaxHealthBonus) * (1f + maxHealthMultiplier);
    public float CurrentHealth { get; private set; }
    public float Armor => armor;
    public float HealthRegenPerSecond => healthRegenPerSecond;

    public void Configure(float configuredMaxHealth, float configuredImmunityDuration)
    {
        baseMaxHealth = configuredMaxHealth;
        flatMaxHealthBonus = 0f;
        maxHealthMultiplier = 0f;
        immunityDuration = configuredImmunityDuration;
        armor = 5f;
        healthRegenPerSecond = .05f;
        CurrentHealth = MaxHealth;
        initialized = true;
    }

    public void ApplyMaxHealthModifier(float value)
    {
        float previousMaxHealth = MaxHealth;
        maxHealthMultiplier += value;
        float newMaxHealth = MaxHealth;
        CurrentHealth = Mathf.Min(CurrentHealth + (newMaxHealth - previousMaxHealth), newMaxHealth);
        RefreshHealthUI();
    }

    public void ApplyFlatMaxHealthModifier(float value)
    {
        float previousMaxHealth = MaxHealth;
        flatMaxHealthBonus = Mathf.Max(0f, flatMaxHealthBonus + value);
        float newMaxHealth = MaxHealth;
        CurrentHealth = Mathf.Min(CurrentHealth + (newMaxHealth - previousMaxHealth), newMaxHealth);
        RefreshHealthUI();
    }

    public void ApplyArmorModifier(float value)
    {
        armor = Mathf.Max(0f, armor + value);
    }

    public void ApplyHealthRegenModifier(float value)
    {
        healthRegenPerSecond = Mathf.Max(0f, healthRegenPerSecond + value);
    }

    void Start()
    {
        if (!initialized)
        {
            return;
        }

        RefreshHealthUI();
    }

    void Update()
    {
        RegenerateHealth();

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
        CurrentHealth -= GetMitigatedDamage(damage);

        RefreshHealthUI();

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
        RefreshHealthUI();
    }

    private void RegenerateHealth()
    {
        if (healthRegenPerSecond <= 0f || CurrentHealth <= 0f || CurrentHealth >= MaxHealth)
        {
            return;
        }

        float previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Min(CurrentHealth + healthRegenPerSecond * Time.deltaTime, MaxHealth);

        if (!Mathf.Approximately(previousHealth, CurrentHealth))
        {
            RefreshHealthUI();
        }
    }

    private float GetMitigatedDamage(float incomingDamage)
    {
        if (incomingDamage <= 0f)
        {
            return 0f;
        }

        if (armor <= 0f)
        {
            return incomingDamage;
        }

        float mitigatedDamage = incomingDamage * (ArmorMitigationDenominator / (ArmorMitigationDenominator + armor));
        return Mathf.Max(1f, mitigatedDamage);
    }

    private void RefreshHealthUI()
    {
        if (UIController.Instance != null)
        {
            UIController.Instance.UpdateHealthSlider();
        }
    }
}
