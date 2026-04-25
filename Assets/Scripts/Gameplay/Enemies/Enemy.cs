using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static event System.Action<Enemy> EnemyKilled;

    private const float KnockbackDuration = 0.1f;
    private static readonly Vector2 ZeroVelocity = Vector2.zero;
    private static readonly Collider2D[] KnockbackPropagationColliderBuffer = new Collider2D[16];
    private static readonly Enemy[] KnockbackPropagationEnemyBuffer = new Enemy[16];

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
    [SerializeField, Min(0f)] private float attackRecoilOnHit = 3f;
    [SerializeField, Min(0f)] private float attackRecoilOnEvade = 4.5f;
    [SerializeField, Min(0f)] private float knockbackPropagationRadius = 1f;
    [SerializeField, Range(0f, 1f)] private float knockbackPropagationForceMultiplier = 0.65f;
    [SerializeField, Range(-1f, 1f)] private float knockbackPropagationForwardDot = 0.05f;
    [SerializeField, Min(0)] private int knockbackPropagationMaxTargets = 4;

    [Header("Rarity Visuals")]
    [SerializeField] private Color uncommonOutlineColor = new Color(0.2f, 0.55f, 1f, 1f);
    [SerializeField] private Color rareOutlineColor = new Color(1f, 0.8f, 0.15f, 1f);
    [SerializeField] private float uncommonOutlineThickness = 1.2f;
    [SerializeField] private float rareOutlineThickness = 1.4f;

    private Vector2 knockbackVelocity;
    private float knockbackTimer;
    private Material defaultMaterial;
    private MaterialPropertyBlock rarityPropertyBlock;
    private Collider2D bodyCollider;

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
        bodyCollider = GetComponent<Collider2D>();
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

        if (playerHealth == null)
        {
            return;
        }

        PlayerHealth.EnemyContactResult contactResult =
            playerHealth.ResolveEnemyContactDamage(runtimeStats.contactDamage, gameObject.GetEntityId());

        if (contactResult == PlayerHealth.EnemyContactResult.NoEffect)
        {
            return;
        }

        ApplyAttackRecoil(collision, contactResult == PlayerHealth.EnemyContactResult.Evaded ? attackRecoilOnEvade : attackRecoilOnHit);
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
        ProcessDrops(powerUpLootTable, lootItem => lootItem.GetAdjustedDropChance(runtimeStats.dropChanceMultiplier), SpawnLoot);
        ProcessDrops(metaLootTable, lootItem => lootItem.GetAdjustedDropChance(GetMetaDropChanceMultiplier(lootItem)), SpawnMetaLoot);
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

    private void ApplyKnockback(Vector2 hitDirection, float force, bool applyResistance = true, bool propagateToNearbyEnemies = true)
    {
        float finalForce = force;
        if (applyResistance)
        {
            float resistance = Mathf.Clamp01(knockbackResistance / 100f);
            finalForce *= 1f - resistance;
        }

        if (finalForce <= 0f || hitDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        Vector2 knockbackDirection = hitDirection.normalized;
        ApplyKnockbackVelocity(knockbackDirection, finalForce);

        if (propagateToNearbyEnemies)
        {
            PropagateKnockbackToNearbyEnemies(knockbackDirection, finalForce, applyResistance);
        }
    }

    private void ApplyKnockbackVelocity(Vector2 direction, float force)
    {
        knockbackVelocity = direction * force;
        knockbackTimer = KnockbackDuration;
    }

    private void PropagateKnockbackToNearbyEnemies(Vector2 direction, float force, bool applyResistance)
    {
        if (knockbackPropagationRadius <= 0f || knockbackPropagationForceMultiplier <= 0f || knockbackPropagationMaxTargets <= 0)
        {
            return;
        }

        float propagatedForce = force * knockbackPropagationForceMultiplier;
        if (propagatedForce <= 0f)
        {
            return;
        }

        Vector2 overlapOrigin = (Vector2)transform.position + direction * (knockbackPropagationRadius * 0.35f);
        ContactFilter2D enemyFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = 1 << gameObject.layer,
            useTriggers = true
        };
        int hitCount = Physics2D.OverlapCircle(
            overlapOrigin,
            knockbackPropagationRadius,
            enemyFilter,
            KnockbackPropagationColliderBuffer);

        int uniqueEnemyCount = 0;
        int pushedEnemyCount = 0;

        for (int i = 0; i < hitCount && pushedEnemyCount < knockbackPropagationMaxTargets; i++)
        {
            Collider2D hitCollider = KnockbackPropagationColliderBuffer[i];
            if (hitCollider == null || hitCollider == bodyCollider)
            {
                continue;
            }

            Enemy otherEnemy = hitCollider.GetComponentInParent<Enemy>();
            if (otherEnemy == null || otherEnemy == this || ContainsEnemyReference(uniqueEnemyCount, otherEnemy))
            {
                continue;
            }

            KnockbackPropagationEnemyBuffer[uniqueEnemyCount++] = otherEnemy;

            Vector2 toOtherEnemy = (Vector2)(otherEnemy.transform.position - transform.position);
            if (toOtherEnemy.sqrMagnitude <= Mathf.Epsilon)
            {
                continue;
            }

            if (Vector2.Dot(direction, toOtherEnemy.normalized) < knockbackPropagationForwardDot)
            {
                continue;
            }

            otherEnemy.ReceivePropagatedKnockback(direction, propagatedForce, applyResistance);
            pushedEnemyCount++;
        }

        for (int i = 0; i < uniqueEnemyCount; i++)
        {
            KnockbackPropagationEnemyBuffer[i] = null;
        }
    }

    private void ReceivePropagatedKnockback(Vector2 direction, float force, bool applyResistance)
    {
        ApplyKnockback(direction, force, applyResistance, propagateToNearbyEnemies: false);
    }

    private static bool ContainsEnemyReference(int count, Enemy target)
    {
        for (int i = 0; i < count; i++)
        {
            if (KnockbackPropagationEnemyBuffer[i] == target)
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyAttackRecoil(Collision2D collision, float recoilForce)
    {
        if (recoilForce <= 0f)
        {
            return;
        }

        Vector2 recoilDirection = transform.position - collision.transform.position;
        if (recoilDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            recoilDirection = transform.position - player.transform.position;
        }

        if (recoilDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            recoilDirection = Vector2.up;
        }

        ApplyKnockback(recoilDirection.normalized, recoilForce, applyResistance: false);
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
        if (lootItem == null)
        {
            return;
        }

        (lootItem.type == MetaLootType.Map ? (System.Action<MetaLootItem>)SpawnMapLoot : SpawnEquipmentLoot)(lootItem);
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

    private void SpawnEquipmentLoot(MetaLootItem lootItem)
    {
        EquipmentBaseCatalog baseCatalog = EquipmentCatalogResources.BaseCatalog;
        EquipmentAffixCatalog affixCatalog = EquipmentCatalogResources.AffixCatalog;
        MapInstance selectedMap = RunData.GetSelectedMapOrDefault();

        if (baseCatalog == null || affixCatalog == null)
        {
            return;
        }

        EquipmentGenerationRequest request = lootItem.equipmentDropSettings.BuildRequest(selectedMap);
        request.itemLevel = EquipmentItemLevelResolver.Resolve(selectedMap, archetypeDefinition);
        EquipmentInstance droppedEquipment = EquipmentGenerator.Generate(
            baseCatalog,
            affixCatalog,
            request,
            lootItem.equipmentDropSettings.CommonWeight,
            lootItem.equipmentDropSettings.UncommonWeight,
            lootItem.equipmentDropSettings.RareWeight);

        if (droppedEquipment == null)
        {
            return;
        }

        EquipmentPickup equipmentPickup = PickupPools.Instance.GetEquipmentPickup();
        equipmentPickup.transform.position = transform.position;
        equipmentPickup.Initialize(droppedEquipment);
    }

    private void CachePlayerReferences()
    {
        player = PlayerController.Instance;
        if (player != null) playerHealth = player.GetComponent<PlayerHealth>();
    }

    private void ResetRuntimeState()
    {
        runtimeStats = BuildRuntimeStats();
        currentHealth = runtimeStats.maxHealth;
        knockbackTimer = 0f;
        knockbackVelocity = ZeroVelocity;
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

    private float GetMetaDropChanceMultiplier(MetaLootItem lootItem)
    {
        float multiplier = runtimeStats.dropChanceMultiplier;
        EquipmentStatSummary summary = MetaProgressionService.GetEquippedEquipmentStatSummary();
        EquipmentStatType? bonusStatType = GetMetaDropBonusStat(lootItem);
        AtlasEffectType? atlasEffectType = GetMetaDropAtlasEffectType(lootItem);
        EquipmentStatSummaryEntry entry = bonusStatType.HasValue ? summary?.GetEntry(bonusStatType.Value) : null;
        float equipmentIncrease = entry != null && entry.HasPercentValue ? entry.percentValue : 0f;
        float atlasIncrease = atlasEffectType.HasValue ? MetaProgressionService.GetAtlasEffectValue(atlasEffectType.Value) / 100f : 0f;
        return multiplier * Mathf.Max(0f, 1f + equipmentIncrease + atlasIncrease);
    }

    private int GetScaledPackBound(int baseValue, float qualityMultiplier)
    {
        float scaledValue = Mathf.Max(1f, baseValue * Mathf.Max(0f, qualityMultiplier));
        return Mathf.Max(1, Mathf.RoundToInt(scaledValue));
    }

    private bool TryGetActivePlayer() => (player != null || TryCachePlayerReferences()) && player.gameObject.activeSelf;

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

    private void DespawnWithoutRewards() => ReturnToPool();

    private void StopMoving() { if (rb != null) rb.linearVelocity = ZeroVelocity; }

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
        StopMoving();

        // Normal deaths and far-distance cleanup both return through the same pool path.
        if (poolOwner != null && poolPrefabKey != null)
        {
            poolOwner.ReturnEnemyToPool(this, poolPrefabKey);
            return;
        }

        Destroy(gameObject);
    }

    private bool TryCachePlayerReferences()
    {
        CachePlayerReferences();
        return player != null;
    }

    private static EquipmentStatType? GetMetaDropBonusStat(MetaLootItem lootItem) =>
        lootItem?.type switch
        {
            MetaLootType.Map => EquipmentStatType.MapDropChance,
            MetaLootType.Equipment => EquipmentStatType.EquipmentDropChance,
            _ => null
        };

    // Equipment and atlas both contribute "increased drop chance" style bonuses, so they share one multiplier path.
    private static AtlasEffectType? GetMetaDropAtlasEffectType(MetaLootItem lootItem) =>
        lootItem?.type switch
        {
            MetaLootType.Map => AtlasEffectType.MapDropChancePercent,
            MetaLootType.Equipment => AtlasEffectType.EquipmentDropChancePercent,
            _ => null
        };

    private void ProcessDrops<TLoot>(
        IReadOnlyList<TLoot> lootTable,
        System.Func<TLoot, float> getDropChance,
        System.Action<TLoot> spawnLoot)
        where TLoot : class
    {
        if (lootTable == null || getDropChance == null || spawnLoot == null)
        {
            return;
        }

        for (int i = 0; i < lootTable.Count; i++)
        {
            TLoot lootItem = lootTable[i];

            if (lootItem != null && Random.Range(0f, 100f) <= getDropChance(lootItem))
            {
                spawnLoot(lootItem);
            }
        }
    }
}
