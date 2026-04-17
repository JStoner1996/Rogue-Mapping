using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static event System.Action<Enemy> EnemyKilled;

    private const float KnockbackDuration = 0.1f;

    private struct RuntimeStats
    {
        public float maxHealth;
        public float moveSpeed;
        public float contactDamage;
        public int experienceWorth;
        public float dropChanceMultiplier;
    }

    [Header("Enemy Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject destroyEffect;
    [SerializeField] private Material rarityOutlineMaterial;

    [Header("Archetype")]
    [SerializeField] private EnemyArchetypeDefinition archetypeDefinition;

    [Header("Enemy Stats")]
    [SerializeField] private int experienceWorth;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float health;

    [Header("Pack Spawning")]
    [SerializeField, Min(1)] private int packSizeMin = 1;
    [SerializeField, Min(1)] private int packSizeMax = 1;

    [Header("Knockback")]
    [SerializeField, Range(0f, 100f)]
    private float knockbackResistance = 0f;

    [Header("Rarity Visuals")]
    [SerializeField] private Color uncommonOutlineColor = new Color(0.2f, 0.55f, 1f, 1f);
    [SerializeField] private Color rareOutlineColor = new Color(1f, 0.8f, 0.15f, 1f);
    [SerializeField] private float uncommonOutlineThickness = 1.2f;
    [SerializeField] private float rareOutlineThickness = 1.4f;

    private Vector2 knockbackVelocity;
    private float knockbackTimer;
    private Material defaultMaterial;
    private MaterialPropertyBlock rarityPropertyBlock;

    [Header("Drops")]
    [SerializeField] private List<LootItem> powerUpLootTable = new List<LootItem>();
    [SerializeField] private List<MetaLootItem> metaLootTable = new List<MetaLootItem>();

    public EnemyRarity Rarity { get; private set; } = EnemyRarity.Normal;
    public EnemyArchetypeDefinition ArchetypeDefinition => archetypeDefinition;
    public EnemyArchetype Archetype => archetypeDefinition != null ? archetypeDefinition.Archetype : EnemyArchetype.Fodder;
    private PlayerController player;
    private PlayerHealth playerHealth;
    private WorldChunkManager worldChunkManager;
    private EnemySpawner poolOwner;
    private GameObject poolPrefabKey;
    private RuntimeStats runtimeStats;
    private float currentHealth;
    private EnemySpawnContext spawnContext = EnemySpawnContext.Default;

    void Awake()
    {
        EnsureRarityMaterial();
        ResetRuntimeState();
    }

    void OnEnable()
    {
        ResetRuntimeState();
    }

    void FixedUpdate()
    {
        if (!TryGetActivePlayer())
        {
            StopMoving();
            return;
        }

        if (ShouldDespawnForChunkDistance())
        {
            DespawnWithoutRewards();
            return;
        }

        UpdateFacing();
        rb.linearVelocity = GetMovementVelocity();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (playerHealth == null)
        {
            CachePlayerReferences();
        }

        playerHealth?.TakeDamage(runtimeStats.contactDamage);
    }

    public void TakeDamage(float damage, Vector2? hitDirection = null, float knockbackForce = 0f)
    {
        currentHealth -= damage;

        DamageNumberController.Instance.CreateNumber(damage, transform.position);
        if (hitDirection.HasValue && knockbackForce > 0f)
        {
            ApplyKnockback(hitDirection.Value, knockbackForce);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Initialize(EnemySpawnContext context)
    {
        spawnContext = context;
        Rarity = context.rarity;
        ResetRuntimeState();
    }

    public void ConfigurePool(EnemySpawner owner, GameObject prefabKey)
    {
        poolOwner = owner;
        poolPrefabKey = prefabKey;
    }

    public int RollPackSize(float qualityMultiplier)
    {
        if (archetypeDefinition != null && !archetypeDefinition.UsesAmbientSpawnWeight)
        {
            return 1;
        }

        int minimum = GetScaledPackBound(packSizeMin, qualityMultiplier);
        int maximum = GetScaledPackBound(Mathf.Max(packSizeMin, packSizeMax), qualityMultiplier);

        if (maximum < minimum)
        {
            maximum = minimum;
        }

        return Random.Range(minimum, maximum + 1);
    }

    private void Die()
    {
        DropExperience();
        DropPowerUps();
        DropMetaItems();
        EnemyKilled?.Invoke(this);
        PlayDeathEffects();
        ReturnToPool();
    }

    private void DropExperience()
    {
        ExpCrystal xp = PickupPools.Instance.GetXP();
        xp.transform.position = transform.position;
        xp.Init(runtimeStats.experienceWorth);
    }

    private void DropPowerUps()
    {
        foreach (LootItem lootItem in powerUpLootTable)
        {
            if (lootItem == null)
            {
                continue;
            }

            float dropChance = lootItem.GetAdjustedDropChance(runtimeStats.dropChanceMultiplier);

            if (Random.Range(0f, 100f) > dropChance)
            {
                continue;
            }

            SpawnLoot(lootItem);
        }
    }

    private void DropMetaItems()
    {
        foreach (MetaLootItem lootItem in metaLootTable)
        {
            if (lootItem == null)
            {
                continue;
            }

            float dropChance = lootItem.GetAdjustedDropChance(runtimeStats.dropChanceMultiplier);

            if (Random.Range(0f, 100f) > dropChance)
            {
                continue;
            }

            SpawnMetaLoot(lootItem);
        }
    }

    private void ApplyKnockback(Vector2 hitDirection, float force)
    {
        float resistance = Mathf.Clamp01(knockbackResistance / 100f);
        float finalForce = force * (1f - resistance);

        knockbackVelocity = hitDirection.normalized * finalForce;
        knockbackTimer = KnockbackDuration;
    }

    private void SpawnLoot(LootItem lootItem)
    {
        if (lootItem == null)
        {
            return;
        }

        switch (lootItem.type)
        {
            case PowerUpLootType.Health:
                HealthPickup health = PickupPools.Instance.GetHealth();
                health.transform.position = transform.position;
                break;

            case PowerUpLootType.Magnet:
                Magnet magnet = PickupPools.Instance.GetMagnet();
                magnet.transform.position = transform.position;
                break;

            case PowerUpLootType.Bomb:
                Bomb bomb = PickupPools.Instance.GetBomb();
                bomb.transform.position = transform.position;
                break;
        }
    }

    private void SpawnMetaLoot(MetaLootItem lootItem)
    {
        switch (lootItem.type)
        {
            case MetaLootType.Map:
                SpawnMapLoot(lootItem);
                break;
        }
    }

    private void SpawnMapLoot(MetaLootItem lootItem)
    {
        MapInstance droppedMap = MapGenerator.CreateDroppedMap(
            RunData.GetSelectedMapOrDefault().Tier,
            lootItem.mapDropSettings);

        if (droppedMap == null)
        {
            return;
        }

        MapPickup mapPickup = PickupPools.Instance.GetMapPickup();
        mapPickup.transform.position = transform.position;
        mapPickup.Initialize(droppedMap);
    }

    private void CachePlayerReferences()
    {
        player = PlayerController.Instance;

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void ResetRuntimeState()
    {
        runtimeStats = BuildRuntimeStats();
        currentHealth = runtimeStats.maxHealth;
        knockbackTimer = 0f;
        knockbackVelocity = Vector2.zero;
        ApplyRarityVisuals();
    }

    private RuntimeStats BuildRuntimeStats()
    {
        float archetypeHealthMultiplier = archetypeDefinition != null ? archetypeDefinition.HealthMultiplier : 1f;
        float archetypeDamageMultiplier = archetypeDefinition != null ? archetypeDefinition.DamageMultiplier : 1f;
        float archetypeMoveSpeedMultiplier = archetypeDefinition != null ? archetypeDefinition.MoveSpeedMultiplier : 1f;
        float archetypeExperienceMultiplier = archetypeDefinition != null ? archetypeDefinition.ExperienceMultiplier : 1f;
        float archetypeDropChanceMultiplier = archetypeDefinition != null ? archetypeDefinition.DropChanceMultiplier : 1f;

        return new RuntimeStats
        {
            maxHealth = health * archetypeHealthMultiplier * spawnContext.healthMultiplier,
            moveSpeed = moveSpeed * archetypeMoveSpeedMultiplier * spawnContext.moveSpeedMultiplier,
            contactDamage = damage * archetypeDamageMultiplier * spawnContext.damageMultiplier,
            experienceWorth = Mathf.RoundToInt(experienceWorth * archetypeExperienceMultiplier * spawnContext.experienceMultiplier),
            dropChanceMultiplier = archetypeDropChanceMultiplier * spawnContext.dropChanceMultiplier,
        };
    }

    private int GetScaledPackBound(int baseValue, float qualityMultiplier)
    {
        float scaledValue = Mathf.Max(1f, baseValue * Mathf.Max(0f, qualityMultiplier));
        return Mathf.Max(1, Mathf.RoundToInt(scaledValue));
    }

    private bool TryGetActivePlayer()
    {
        if (player == null)
        {
            CachePlayerReferences();
        }

        return player != null && player.gameObject.activeSelf;
    }

    // Ambient enemies are cleaned up once they fall too many chunks behind the player.
    private bool ShouldDespawnForChunkDistance()
    {
        if (archetypeDefinition == null || archetypeDefinition.SpawnRole != EnemySpawnRole.Ambient)
        {
            return false;
        }

        if (!TryGetWorldChunkManager(out WorldChunkManager chunkManager))
        {
            return false;
        }

        int despawnChunkDistance = chunkManager.AmbientEnemyDespawnChunkDistance;
        if (despawnChunkDistance <= 0 || player == null)
        {
            return false;
        }

        ChunkCoordinate enemyChunk = ChunkWorldUtility.GetChunkCoordinate(
            transform.position,
            chunkManager.ChunkSizeTiles,
            chunkManager.TileSize);
        ChunkCoordinate playerChunk = ChunkWorldUtility.GetChunkCoordinate(
            player.transform.position,
            chunkManager.ChunkSizeTiles,
            chunkManager.TileSize);

        return ChunkWorldUtility.GetChebyshevDistance(enemyChunk, playerChunk) > despawnChunkDistance;
    }

    private void DespawnWithoutRewards()
    {
        ReturnToPool();
    }

    private void StopMoving()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void UpdateFacing()
    {
        if (spriteRenderer == null || player == null)
        {
            return;
        }

        spriteRenderer.flipX = player.transform.position.x > transform.position.x;
    }

    private Vector2 GetMovementVelocity()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
            return knockbackVelocity;
        }

        Vector2 direction = (player.transform.position - transform.position).normalized;
        return direction * runtimeStats.moveSpeed;
    }

    private void PlayDeathEffects()
    {
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, transform.rotation);
        }

        AudioManager.Instance.Play(SoundType.EnemyDie);
    }

    private void EnsureRarityMaterial()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        defaultMaterial = spriteRenderer.sharedMaterial;
        rarityPropertyBlock ??= new MaterialPropertyBlock();

        if (rarityOutlineMaterial != null)
        {
            return;
        }

        Shader outlineShader = Shader.Find("Custom/SpriteRarityOutline");

        if (outlineShader != null)
        {
            rarityOutlineMaterial = new Material(outlineShader);
        }
    }

    private void ApplyRarityVisuals()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color outlineColor = Color.white;
        float outlineThickness = 0f;

        switch (Rarity)
        {
            case EnemyRarity.Uncommon:
                outlineColor = uncommonOutlineColor;
                outlineThickness = uncommonOutlineThickness;
                break;

            case EnemyRarity.Rare:
                outlineColor = rareOutlineColor;
                outlineThickness = rareOutlineThickness;
                break;
        }

        if (rarityOutlineMaterial == null || outlineThickness <= 0f)
        {
            spriteRenderer.sharedMaterial = defaultMaterial;
            spriteRenderer.SetPropertyBlock(null);
            return;
        }

        spriteRenderer.sharedMaterial = rarityOutlineMaterial;
        rarityPropertyBlock.Clear();
        rarityPropertyBlock.SetColor("_OutlineColor", outlineColor);
        rarityPropertyBlock.SetFloat("_OutlineThickness", outlineThickness);
        spriteRenderer.SetPropertyBlock(rarityPropertyBlock);
    }

    private bool TryGetWorldChunkManager(out WorldChunkManager chunkManager)
    {
        if (worldChunkManager == null)
        {
            worldChunkManager = WorldChunkManager.Instance ?? FindAnyObjectByType<WorldChunkManager>();
        }

        chunkManager = worldChunkManager;
        return chunkManager != null;
    }

    private void ReturnToPool()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Normal deaths and far-distance cleanup both return through the same pool path.
        if (poolOwner != null && poolPrefabKey != null)
        {
            poolOwner.ReturnEnemyToPool(this, poolPrefabKey);
            return;
        }

        Destroy(gameObject);
    }
}
