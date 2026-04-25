using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    private const float ArmorMitigationDenominator = 100f;
    private const float EvasionScalingDenominator = 100f;
    private const float MaximumEvadeChance = 0.75f;
    private const float EvasionEntropyJitterMagnitude = 0.10f;
    private const float DefaultBarrierRegenDelay = 5f;
    private const float DefaultBarrierRegenPercentPerSecond = 0.20f;

    private enum IncomingDamageKind
    {
        Generic,
        EnemyContact,
    }

    public enum EnemyContactResult
    {
        NoEffect,
        Evaded,
        Hit,
    }

    private float baseMaxHealth;
    private float flatMaxHealthBonus;
    private float maxHealthMultiplier;
    private float baseMaxBarrier;
    private float flatMaxBarrierBonus;
    private float maxBarrierMultiplier;
    private float immunityDuration;
    private bool initialized;
    private float armor;
    private float evasion;
    private float healthRegenPerSecond;
    private float barrierRegenDelay;
    private float barrierRegenPercentPerSecond;
    private float barrierRegenTimer;
    private readonly Dictionary<EntityId, float> evasionEntropyByAttacker = new Dictionary<EntityId, float>();

    private bool isImmune;
    private float immunityTimer;

    public float MaxHealth => (baseMaxHealth + flatMaxHealthBonus) * (1f + maxHealthMultiplier);
    public float MaxBarrier => (baseMaxBarrier + flatMaxBarrierBonus) * (1f + maxBarrierMultiplier);
    public float CurrentHealth { get; private set; }
    public float CurrentBarrier { get; private set; }
    public float Armor => armor;
    public float ArmorMitigationFraction => armor <= 0f ? 0f : armor / (ArmorMitigationDenominator + armor);
    public float Evasion => evasion;
    public float EvadeChance => Mathf.Min(MaximumEvadeChance, evasion / (evasion + EvasionScalingDenominator));
    public float HealthRegenPerSecond => healthRegenPerSecond;

    public void Configure(float configuredMaxHealth, float configuredImmunityDuration)
    {
        baseMaxHealth = configuredMaxHealth;
        flatMaxHealthBonus = 0f;
        maxHealthMultiplier = 0f;
        baseMaxBarrier = 0f;
        flatMaxBarrierBonus = 0f;
        maxBarrierMultiplier = 0f;
        immunityDuration = configuredImmunityDuration;
        armor = 5f;
        evasion = 5f;
        healthRegenPerSecond = .05f;
        barrierRegenDelay = DefaultBarrierRegenDelay;
        barrierRegenPercentPerSecond = DefaultBarrierRegenPercentPerSecond;
        barrierRegenTimer = 0f;
        CurrentHealth = MaxHealth;
        CurrentBarrier = MaxBarrier;
        initialized = true;
    }

    public void ApplyMaxHealthModifier(float value)
    {
        float previousMaxHealth = MaxHealth;
        maxHealthMultiplier += value;
        AdjustCurrentHealthForMaxHealthChange(previousMaxHealth);
    }

    public void ApplyFlatMaxHealthModifier(float value)
    {
        float previousMaxHealth = MaxHealth;
        flatMaxHealthBonus = Mathf.Max(0f, flatMaxHealthBonus + value);
        AdjustCurrentHealthForMaxHealthChange(previousMaxHealth);
    }

    public void ApplyArmorModifier(float value)
    {
        armor = Mathf.Max(0f, armor + value);
    }

    public void ApplyBarrierModifier(float value)
    {
        float previousMaxBarrier = MaxBarrier;
        maxBarrierMultiplier += value;
        AdjustCurrentBarrierForMaxBarrierChange(previousMaxBarrier);
    }

    public void ApplyFlatBarrierModifier(float value)
    {
        float previousMaxBarrier = MaxBarrier;
        flatMaxBarrierBonus = Mathf.Max(0f, flatMaxBarrierBonus + value);
        AdjustCurrentBarrierForMaxBarrierChange(previousMaxBarrier);
    }

    public void ApplyEvasionModifier(float value)
    {
        evasion = Mathf.Max(0f, evasion + value);
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
        RegenerateBarrier();
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
        ApplyIncomingDamage(damage, IncomingDamageKind.Generic);
    }

    public void TakeEnemyContactDamage(float damage)
    {
        ApplyIncomingDamage(damage, IncomingDamageKind.EnemyContact);
    }

    public void TakeEnemyContactDamage(float damage, EntityId attackerId)
    {
        ApplyIncomingDamage(damage, IncomingDamageKind.EnemyContact, attackerId);
    }

    public EnemyContactResult ResolveEnemyContactDamage(float damage, EntityId attackerId)
    {
        if (isImmune)
        {
            return EnemyContactResult.NoEffect;
        }

        if (damage <= 0f)
        {
            return EnemyContactResult.NoEffect;
        }

        DamageResolution resolution = ResolveDamageAgainstBarrier(damage);
        if (resolution.remainingDamage <= 0f)
        {
            RegisterSuccessfulHit();
            RefreshHealthUI();
            return EnemyContactResult.Hit;
        }

        if (ShouldEvadeEnemyContact(attackerId))
        {
            return EnemyContactResult.Evaded;
        }

        ApplyLifeDamage(resolution.remainingDamage);
        return EnemyContactResult.Hit;
    }

    private void ApplyIncomingDamage(float damage, IncomingDamageKind damageKind)
    {
        ApplyIncomingDamage(damage, damageKind, EntityId.None);
    }

    private void ApplyIncomingDamage(float damage, IncomingDamageKind damageKind, EntityId attackerId)
    {
        if (isImmune)
        {
            return;
        }

        if (damage <= 0f)
        {
            return;
        }

        if (damageKind == IncomingDamageKind.EnemyContact)
        {
            ResolveEnemyContactDamage(damage, attackerId);
            return;
        }

        DamageResolution resolution = ResolveDamageAgainstBarrier(damage);
        if (resolution.remainingDamage <= 0f)
        {
            RegisterSuccessfulHit();
            RefreshHealthUI();
            return;
        }

        ApplyLifeDamage(resolution.remainingDamage);
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

    private void RegenerateBarrier()
    {
        if (MaxBarrier <= 0f || CurrentBarrier >= MaxBarrier || CurrentHealth <= 0f)
        {
            return;
        }

        if (barrierRegenTimer > 0f)
        {
            barrierRegenTimer -= Time.deltaTime;
            return;
        }

        float regenAmount = MaxBarrier * barrierRegenPercentPerSecond * Time.deltaTime;
        if (regenAmount <= 0f)
        {
            return;
        }

        float previousBarrier = CurrentBarrier;
        CurrentBarrier = Mathf.Min(CurrentBarrier + regenAmount, MaxBarrier);

        if (!Mathf.Approximately(previousBarrier, CurrentBarrier))
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

    private bool ShouldEvadeEnemyContact(EntityId attackerId)
    {
        if (evasion <= 0f)
        {
            return false;
        }

        float hitChance = 1f - EvadeChance;
        float entropy = GetEvasionEntropy(attackerId) + hitChance;
        float jitter = Random.Range(-EvasionEntropyJitterMagnitude, EvasionEntropyJitterMagnitude);

        if (entropy + jitter >= 1f)
        {
            SetEvasionEntropy(attackerId, Mathf.Max(0f, entropy - 1f));
            return false;
        }

        SetEvasionEntropy(attackerId, entropy);
        return true;
    }

    private float GetEvasionEntropy(EntityId attackerId)
    {
        return attackerId != EntityId.None && evasionEntropyByAttacker.TryGetValue(attackerId, out float entropy) ? entropy : 0f;
    }

    private void SetEvasionEntropy(EntityId attackerId, float entropy)
    {
        if (attackerId == EntityId.None)
        {
            return;
        }

        evasionEntropyByAttacker[attackerId] = Mathf.Clamp01(entropy);
    }

    private DamageResolution ResolveDamageAgainstBarrier(float damage)
    {
        if (damage <= 0f || CurrentBarrier <= 0f)
        {
            return new DamageResolution(0f, damage);
        }

        float absorbedDamage = Mathf.Min(CurrentBarrier, damage);
        CurrentBarrier -= absorbedDamage;
        ResetBarrierRegenTimer();
        return new DamageResolution(absorbedDamage, damage - absorbedDamage);
    }

    private void ApplyLifeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        RegisterSuccessfulHit();
        CurrentHealth -= GetMitigatedDamage(damage);

        RefreshHealthUI();

        if (CurrentHealth > 0f)
        {
            return;
        }

        gameObject.SetActive(false);
        GameManager.Instance.GameOver();
    }

    private void RefreshHealthUI() => UIController.Instance?.UpdateHealthSlider();

    private void AdjustCurrentHealthForMaxHealthChange(float previousMaxHealth)
    {
        float newMaxHealth = MaxHealth;
        CurrentHealth = Mathf.Min(CurrentHealth + (newMaxHealth - previousMaxHealth), newMaxHealth);
        RefreshHealthUI();
    }

    private void AdjustCurrentBarrierForMaxBarrierChange(float previousMaxBarrier)
    {
        float newMaxBarrier = MaxBarrier;
        CurrentBarrier = Mathf.Clamp(CurrentBarrier + (newMaxBarrier - previousMaxBarrier), 0f, newMaxBarrier);
        RefreshHealthUI();
    }

    private void RegisterSuccessfulHit()
    {
        isImmune = true;
        immunityTimer = immunityDuration;
        ResetBarrierRegenTimer();
    }

    private void ResetBarrierRegenTimer()
    {
        barrierRegenTimer = barrierRegenDelay;
    }

    private readonly struct DamageResolution
    {
        public DamageResolution(float barrierAbsorbedDamage, float remainingDamage)
        {
            this.barrierAbsorbedDamage = barrierAbsorbedDamage;
            this.remainingDamage = remainingDamage;
        }

        public readonly float barrierAbsorbedDamage;
        public readonly float remainingDamage;
    }
}
