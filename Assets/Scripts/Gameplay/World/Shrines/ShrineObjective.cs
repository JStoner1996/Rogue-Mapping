using UnityEngine;
using UnityEngine.UI;

// Handles the shared shrine loop of proximity, charging, and one-time activation.
[RequireComponent(typeof(Collider2D))]
public class ShrineObjective : MonoBehaviour
{
    public event System.Action<ShrineObjective> Activated;

    [Header("Shrine")]
    [SerializeField] private ShrineDefinition shrineDefinition;

    [Header("Visual References")]
    [SerializeField] private SpriteRenderer shrineRenderer;
    [SerializeField] private Transform activationRadiusVisualRoot;
    [SerializeField] private GameObject chargeIndicatorRoot;
    [SerializeField] private Slider chargeSlider;
    [SerializeField] private Image chargeFillImage;

    [Header("Visual State")]
    [SerializeField] private Color depletedTint = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField, Min(1f)] private float greaterShrineScaleMultiplier = 1.5f;

    [Header("Debug")]
    [SerializeField, Range(0f, 1f)] private float debugChargeNormalized;

    private EnemySpawner enemySpawner;
    private Collider2D triggerCollider;
    private Vector3 baseShrineVisualScale;
    private Vector3 baseActivationRadiusVisualScale;
    private float baseCircleRadius;
    private Vector2 baseBoxSize;
    private Vector2 baseCapsuleSize;
    private bool playerInside;
    private bool activated;
    private bool isGreaterShrine;
    private float currentCharge;

    public ShrineDefinition Definition => shrineDefinition;
    public float ChargeNormalized => shrineDefinition == null ? 0f : Mathf.Clamp01(currentCharge / shrineDefinition.ChargeDuration);
    public bool IsActivated => activated;
    public bool IsGreaterShrine => isGreaterShrine;

    public void Configure(ShrineDefinition definition, bool greaterShrine = false)
    {
        shrineDefinition = definition;
        isGreaterShrine = greaterShrine;
        activated = false;
        currentCharge = 0f;
        ApplyDefinitionVisuals();
        ApplyScaleVisuals();
        ApplyActivationRadius();
        RefreshChargeVisuals();
    }

    void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        enemySpawner = FindAnyObjectByType<EnemySpawner>();
        CacheBaseValues();
        ConfigureTrigger();
        ApplyDefinitionVisuals();
        ApplyScaleVisuals();
        ApplyActivationRadius();
        RefreshChargeVisuals();
    }

    void OnValidate()
    {
        ApplyDefinitionVisuals();
        RefreshChargeVisuals();
    }

    void Update()
    {
        if (!activated && shrineDefinition != null)
        {
            UpdateCharge();

            if (currentCharge >= shrineDefinition.ChargeDuration)
            {
                ActivateShrine();
            }
        }

        RefreshChargeState();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayerCollider(other))
        {
            return;
        }

        playerInside = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayerCollider(other))
        {
            return;
        }

        playerInside = false;
    }

    public bool TrySpawnEventEnemies(EnemyArchetype archetype, int count)
    {
        if (!TryGetEnemySpawner(out EnemySpawner spawner))
        {
            Debug.LogError($"Shrine '{name}' could not spawn '{archetype}' because no EnemySpawner was found.");
            return false;
        }

        return spawner.SpawnEventEnemies(archetype, count, transform.position);
    }

    public void ApplySpawnerModifier(EnemySpawnerModifierType modifierType, float additiveValue, float durationSeconds)
    {
        if (!TryGetEnemySpawner(out EnemySpawner spawner))
        {
            Debug.LogError($"Shrine '{name}' could not apply '{modifierType}' because no EnemySpawner was found.");
            return;
        }

        spawner.AddRuntimeModifier(modifierType, additiveValue, durationSeconds);
        ShrineAtlasRuntime.RegisterActiveShrineBuff(durationSeconds);
    }

    public float GetEffectMultiplier()
    {
        float atlasMultiplier = 1f + MetaProgressionService.GetAtlasEffectValue(AtlasEffectType.ShrineEffectPercent) / 100f;
        float greaterMultiplier = isGreaterShrine ? 2f : 1f;
        return Mathf.Max(0f, atlasMultiplier) * greaterMultiplier;
    }

    public float GetDurationMultiplier()
    {
        float atlasMultiplier = 1f + MetaProgressionService.GetAtlasEffectValue(AtlasEffectType.ShrineDurationPercent) / 100f;
        return Mathf.Max(0f, atlasMultiplier);
    }

    public float ScaleEffectValue(float value) => value * GetEffectMultiplier();

    public float ScaleDuration(float durationSeconds) => durationSeconds * GetDurationMultiplier();

    public int ScaleEffectCount(int count)
    {
        return Mathf.Max(1, Mathf.RoundToInt(count * GetEffectMultiplier()));
    }

    // Charge rises while the player is inside and drains when they step out.
    private void UpdateCharge()
    {
        if (playerInside)
        {
            currentCharge = Mathf.Min(
                shrineDefinition.ChargeDuration,
                currentCharge + Time.deltaTime);
            return;
        }

        currentCharge = Mathf.Max(
            0f,
            currentCharge - Time.deltaTime * GetDischargeRate());
    }

    // Activation is a one-time handoff to the definition-driven shrine effect.
    private void ActivateShrine()
    {
        activated = true;
        currentCharge = shrineDefinition.ChargeDuration;
        shrineDefinition.Effect?.Activate(this);
        ActivateAdditionalShrineEffects();
        Activated?.Invoke(this);
        PlayCompletionSound();
        ApplyActivatedVisuals();
        RefreshChargeVisuals();
    }

    private void ActivateAdditionalShrineEffects()
    {
        int additionalEffectCount = Mathf.Max(
            0,
            Mathf.RoundToInt(MetaProgressionService.GetAtlasEffectValue(AtlasEffectType.AdditionalShrineEffects)));

        if (additionalEffectCount <= 0)
        {
            return;
        }

        WorldChunkManager chunkManager = WorldChunkManager.Instance ?? FindAnyObjectByType<WorldChunkManager>();
        if (chunkManager == null)
        {
            return;
        }

        for (int i = 0; i < additionalEffectCount; i++)
        {
            ShrineDefinition additionalDefinition = chunkManager.GetRandomShrineDefinition(shrineDefinition);
            if (additionalDefinition?.Effect == null)
            {
                continue;
            }

            additionalDefinition.Effect.Activate(this);
        }
    }

    private float GetDischargeRate()
    {
        if (shrineDefinition == null)
        {
            return 0f;
        }

        return shrineDefinition.ChargeDuration / shrineDefinition.DischargeDuration;
    }

    private bool IsPlayerCollider(Collider2D other)
    {
        if (other == null)
        {
            return false;
        }

        return other.CompareTag("Player") || other.GetComponent<PlayerController>() != null;
    }

    private void ConfigureTrigger()
    {
        if (triggerCollider == null)
        {
            return;
        }

        triggerCollider.isTrigger = true;
    }

    private void CacheBaseValues()
    {
        baseShrineVisualScale = shrineRenderer != null ? shrineRenderer.transform.localScale : Vector3.one;
        baseActivationRadiusVisualScale = activationRadiusVisualRoot != null ? activationRadiusVisualRoot.localScale : Vector3.one;

        if (triggerCollider is CircleCollider2D circleCollider)
        {
            baseCircleRadius = circleCollider.radius;
            return;
        }

        if (triggerCollider is BoxCollider2D boxCollider)
        {
            baseBoxSize = boxCollider.size;
            return;
        }

        if (triggerCollider is CapsuleCollider2D capsuleCollider)
        {
            baseCapsuleSize = capsuleCollider.size;
        }
    }

    private void ApplyDefinitionVisuals()
    {
        if (shrineDefinition == null)
        {
            return;
        }

        if (shrineRenderer != null)
        {
            shrineRenderer.sprite = shrineDefinition.ShrineSprite;

            if (!activated)
            {
                shrineRenderer.color = shrineDefinition.ShrineTint;
            }
        }

        if (chargeFillImage != null)
        {
            chargeFillImage.color = shrineDefinition.ShrineTint;
        }
    }

    private void ApplyScaleVisuals()
    {
        if (shrineRenderer != null)
        {
            shrineRenderer.transform.localScale = baseShrineVisualScale * (isGreaterShrine ? greaterShrineScaleMultiplier : 1f);
        }
    }

    private void ApplyActivationRadius()
    {
        if (triggerCollider == null)
        {
            return;
        }

        float atlasMultiplier = 1f + MetaProgressionService.GetAtlasEffectValue(AtlasEffectType.ShrineActivationRadiusPercent) / 100f;
        float radiusMultiplier = Mathf.Max(0f, atlasMultiplier);

        if (activationRadiusVisualRoot != null)
        {
            activationRadiusVisualRoot.localScale = baseActivationRadiusVisualScale * radiusMultiplier;
        }

        if (triggerCollider is CircleCollider2D circleCollider)
        {
            circleCollider.radius = baseCircleRadius * radiusMultiplier;
            return;
        }

        if (triggerCollider is BoxCollider2D boxCollider)
        {
            boxCollider.size = baseBoxSize * radiusMultiplier;
            return;
        }

        if (triggerCollider is CapsuleCollider2D capsuleCollider)
        {
            capsuleCollider.size = baseCapsuleSize * radiusMultiplier;
        }
    }

    private void RefreshChargeVisuals()
    {
        if (chargeIndicatorRoot != null)
        {
            chargeIndicatorRoot.SetActive(!activated && shrineDefinition != null && ChargeNormalized > 0f);
        }

        if (chargeSlider == null)
        {
            return;
        }

        chargeSlider.normalizedValue = ChargeNormalized;
    }

    private void RefreshDebugCharge() => debugChargeNormalized = ChargeNormalized;

    private void ApplyActivatedVisuals()
    {
        if (shrineRenderer != null)
        {
            shrineRenderer.color = depletedTint;
        }
    }

    private void PlayCompletionSound() => AudioManager.Instance?.Play(SoundType.ShrineComplete);

    private bool TryGetEnemySpawner(out EnemySpawner spawner)
    {
        if (enemySpawner == null)
        {
            enemySpawner = FindAnyObjectByType<EnemySpawner>();
        }

        spawner = enemySpawner;
        return spawner != null;
    }

    private void RefreshChargeState()
    {
        RefreshDebugCharge();
        RefreshChargeVisuals();
    }
}

public static class ShrineAtlasRuntime
{
    private static float activeShrineBuffUntil;

    public static bool HasActiveShrineBuff => Time.time < activeShrineBuffUntil;

    public static void RegisterActiveShrineBuff(float durationSeconds)
    {
        if (durationSeconds <= 0f)
        {
            return;
        }

        activeShrineBuffUntil = Mathf.Max(activeShrineBuffUntil, Time.time + durationSeconds);
    }

    public static void TrySpawnShrineFromEnemyKill(Vector3 position)
    {
        if (!HasActiveShrineBuff)
        {
            return;
        }

        float chance = MetaProgressionService.GetAtlasEffectValue(AtlasEffectType.ShrineBuffKillSpawnChancePercent) / 100f;
        if (chance <= 0f || Random.value > Mathf.Clamp01(chance))
        {
            return;
        }

        WorldChunkManager chunkManager = WorldChunkManager.Instance ?? Object.FindAnyObjectByType<WorldChunkManager>();
        chunkManager?.TrySpawnShrineAt(position);
    }
}
