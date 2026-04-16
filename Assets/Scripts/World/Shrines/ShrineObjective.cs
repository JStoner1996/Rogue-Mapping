using UnityEngine;
using UnityEngine.UI;

// Handles the shared shrine loop of proximity, charging, and one-time activation.
[RequireComponent(typeof(Collider2D))]
public class ShrineObjective : MonoBehaviour
{
    [Header("Shrine")]
    [SerializeField] private ShrineDefinition shrineDefinition;

    [Header("Visual References")]
    [SerializeField] private SpriteRenderer shrineRenderer;
    [SerializeField] private GameObject chargeIndicatorRoot;
    [SerializeField] private Slider chargeSlider;
    [SerializeField] private Image chargeFillImage;

    [Header("Visual State")]
    [SerializeField] private Color depletedTint = new Color(0.45f, 0.45f, 0.45f, 1f);

    [Header("Debug")]
    [SerializeField, Range(0f, 1f)] private float debugChargeNormalized;

    private EnemySpawner enemySpawner;
    private Collider2D triggerCollider;
    private bool playerInside;
    private bool activated;
    private float currentCharge;

    public ShrineDefinition Definition => shrineDefinition;
    public float ChargeNormalized => shrineDefinition == null ? 0f : Mathf.Clamp01(currentCharge / shrineDefinition.ChargeDuration);
    public bool IsActivated => activated;

    void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        enemySpawner = FindAnyObjectByType<EnemySpawner>();
        ConfigureTrigger();
        ApplyDefinitionVisuals();
        RefreshChargeVisuals();
    }

    void OnValidate()
    {
        ApplyDefinitionVisuals();
        RefreshChargeVisuals();
    }

    void Update()
    {
        if (activated || shrineDefinition == null)
        {
            RefreshDebugCharge();
            RefreshChargeVisuals();
        }
        else
        {
            UpdateCharge();
            RefreshDebugCharge();
            RefreshChargeVisuals();

            if (currentCharge >= shrineDefinition.ChargeDuration)
            {
                ActivateShrine();
            }
        }
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
        PlayCompletionSound();
        ApplyActivatedVisuals();
        RefreshChargeVisuals();
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

    private void RefreshDebugCharge()
    {
        debugChargeNormalized = ChargeNormalized;
    }

    private void ApplyActivatedVisuals()
    {
        if (shrineRenderer != null)
        {
            shrineRenderer.color = depletedTint;
        }
    }

    private void PlayCompletionSound()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.Play(SoundType.ShrineComplete);
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
}
