using UnityEngine;

// Handles the shared shrine loop of proximity, charging, and one-time activation.
[RequireComponent(typeof(Collider2D))]
public class ShrineObjective : MonoBehaviour
{
    [Header("Shrine")]
    [SerializeField] private ShrineDefinition shrineDefinition;

    [Header("Visual References")]
    [SerializeField] private SpriteRenderer shrineRenderer;

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
    }

    void OnValidate()
    {
        ApplyDefinitionVisuals();
    }

    void Update()
    {
        if (activated || shrineDefinition == null)
        {
            debugChargeNormalized = ChargeNormalized;
            return;
        }

        UpdateCharge();
        debugChargeNormalized = ChargeNormalized;

        if (currentCharge >= shrineDefinition.ChargeDuration)
        {
            ActivateShrine();
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
        if (enemySpawner == null)
        {
            enemySpawner = FindAnyObjectByType<EnemySpawner>();
        }

        if (enemySpawner == null)
        {
            Debug.LogError($"Shrine '{name}' could not spawn '{archetype}' because no EnemySpawner was found.");
            return false;
        }

        return enemySpawner.SpawnEventEnemies(archetype, count, transform.position);
    }

    public void ApplySpawnerModifier(EnemySpawnerModifierType modifierType, float additiveValue, float durationSeconds)
    {
        if (enemySpawner == null)
        {
            enemySpawner = FindAnyObjectByType<EnemySpawner>();
        }

        if (enemySpawner == null)
        {
            Debug.LogError($"Shrine '{name}' could not apply '{modifierType}' because no EnemySpawner was found.");
            return;
        }

        enemySpawner.AddRuntimeModifier(modifierType, additiveValue, durationSeconds);
    }

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

    private void ActivateShrine()
    {
        activated = true;
        currentCharge = shrineDefinition.ChargeDuration;
        shrineDefinition.Effect?.Activate(this);
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
        if (shrineRenderer == null || shrineDefinition == null)
        {
            return;
        }

        shrineRenderer.sprite = shrineDefinition.ShrineSprite;
        shrineRenderer.color = shrineDefinition.ShrineTint;
    }
}
