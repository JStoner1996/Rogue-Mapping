using System.Collections.Generic;
using UnityEngine;

// Spawns ambient enemy packs over time using archetype weights and map progress scaling.
public class EnemySpawner : MonoBehaviour
{
    private struct RuntimeSpawnerModifier
    {
        public EnemySpawnerModifierType type;
        public float additiveValue;
        public float remainingDuration;
    }

    private struct MapSpawnModifiers
    {
        public float quantity;
        public float quality;
        public float damage;
        public float health;
        public float moveSpeed;
        public float experience;
        public float dropChance;
    }

    private struct RarityProfile
    {
        public float healthMultiplier;
        public float damageMultiplier;
        public float moveSpeedMultiplier;
        public float experienceMultiplier;
    }

    private const float MinimumPackIntervalMultiplier = 0.1f;
    private const float RareChancePerQuality = 0.03f;
    private const float UncommonChancePerQuality = 0.12f;

    private static readonly RarityProfile NormalRarityProfile = CreateRarityProfile(1f, 1f, 1f, 1f);
    private static readonly RarityProfile UncommonRarityProfile = CreateRarityProfile(1.5f, 1.2f, 1.05f, 1.5f);
    private static readonly RarityProfile RareRarityProfile = CreateRarityProfile(2.5f, 1.5f, 1.1f, 2.5f);

    [System.Serializable]
    public class PackEntry
    {
        public GameObject enemyPrefab;
    }

    [Header("Ambient Spawn Pool")]
    [SerializeField] private List<PackEntry> ambientPacks = new List<PackEntry>();

    [Header("Event Spawn Pool")]
    [SerializeField] private List<PackEntry> eventPacks = new List<PackEntry>();

    [Header("Run Modifiers")]
    [SerializeField] private EnemySpawnModifiers modifiers = new EnemySpawnModifiers();

    [Header("Pack Spawning")]
    [SerializeField, Min(0.1f)] private float basePackSpawnInterval = 5f;
    [SerializeField, Min(0f)] private float spawnIntervalReductionPerQuarter = 0.5f;
    [SerializeField, Min(0.1f)] private float minimumPackSpawnInterval = 2f;
    [SerializeField, Min(0f)] private float packSpawnRadius = 1.25f;

    [Header("Chunk-Aware Spawning")]
    [SerializeField, Min(0f)] private float ambientSpawnMinimumWorldDistance = 12f;
    [SerializeField, Min(0f)] private float ambientSpawnMaximumWorldDistance = 22f;
    [SerializeField, Min(0)] private int ambientSpawnMaxAttempts = 8;

    [Header("Pooling")]
    [SerializeField, Min(1)] private int initialPoolSizePerEnemy = 8;

    private MapSpawnModifiers mapModifiers;
    private PlayerController player;
    private WorldChunkManager worldChunkManager;
    private EnemyPools enemyPools;
    private float spawnTimer;
    private readonly List<RuntimeSpawnerModifier> temporaryRuntimeModifiers = new List<RuntimeSpawnerModifier>();
    private readonly Dictionary<EnemySpawnerModifierType, float> persistentRuntimeModifiers = new Dictionary<EnemySpawnerModifierType, float>();

    void Awake()
    {
        ApplyRunModifiers();
        BuildEnemyPools();
    }

    void Update()
    {
        UpdateTemporaryModifiers();

        if (!CanRunSpawner())
        {
            return;
        }

        if (!ShouldSpawn())
        {
            return;
        }

        int packCount = GetPackCountPerSpawn();
        for (int i = 0; i < packCount; i++)
        {
            PackEntry selectedEntry = RollAmbientSpawnEntry();
            if (selectedEntry == null)
            {
                return;
            }

            SpawnPack(selectedEntry);
        }
    }

    private bool CanRunSpawner()
    {
        return TryGetActivePlayer()
            && EnemySpawnEntryUtility.HasWeightedEntries(ambientPacks, GetAmbientSpawnWeight)
            && TryGetWorldChunkManager(out _);
    }

    private bool TryGetActivePlayer() => (player ??= PlayerController.Instance) != null && player.gameObject.activeSelf;

    private bool ShouldSpawn()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer < GetCurrentPackSpawnInterval())
        {
            return false;
        }

        spawnTimer = 0f;
        return true;
    }

    private float GetCurrentPackSpawnInterval()
    {
        int completedThresholds = Mathf.FloorToInt(GetVictoryCompletionRate() / 0.25f);
        float intervalReduction = completedThresholds * spawnIntervalReductionPerQuarter;
        float naturalInterval = Mathf.Max(minimumPackSpawnInterval, basePackSpawnInterval - intervalReduction);
        float intervalMultiplier = Mathf.Max(MinimumPackIntervalMultiplier, 1f + GetRuntimeModifierSum(EnemySpawnerModifierType.PackSpawnInterval));
        return Mathf.Max(minimumPackSpawnInterval, naturalInterval * intervalMultiplier);
    }

    private float GetVictoryCompletionRate()
    {
        if (RunData.SelectedMap == null || GameManager.Instance == null || RunData.SelectedMap.VictoryTarget <= 0)
        {
            return 0f;
        }

        return RunData.SelectedMap.VictoryConditionType switch
        {
            VictoryConditionType.Time => Mathf.Clamp01(GameManager.Instance.GameTime / (RunData.SelectedMap.VictoryTarget * 60f)),
            VictoryConditionType.Kills => Mathf.Clamp01((float)GameManager.Instance.EnemyKills / RunData.SelectedMap.VictoryTarget),
            _ => 0f,
        };
    }

    private int GetPackCountPerSpawn()
    {
        float quantityMultiplier = Mathf.Max(0f, GetEffectiveModifier(EnemySpawnerModifierType.EnemyQuantity));
        int guaranteedPacks = Mathf.FloorToInt(quantityMultiplier);
        float fractionalPackChance = quantityMultiplier - guaranteedPacks;
        int packCount = guaranteedPacks;

        if (Random.value < fractionalPackChance)
        {
            packCount++;
        }

        return Mathf.Max(1, packCount);
    }

    private PackEntry RollAmbientSpawnEntry() => EnemySpawnEntryUtility.RollWeightedEntry(ambientPacks, GetAmbientSpawnWeight);

    private float GetAmbientSpawnWeight(PackEntry entry)
    {
        float baseWeight = EnemySpawnEntryUtility.GetAmbientSpawnWeight(entry);
        if (baseWeight <= 0f || !EnemySpawnEntryUtility.TryGetPackDefinition(entry, out _, out EnemyArchetypeDefinition archetypeDefinition))
        {
            return baseWeight;
        }

        return baseWeight * GetArchetypeSpawnWeightMultiplier(archetypeDefinition.Archetype);
    }

    public bool SpawnFinalBosses(int count) => SpawnEventEnemies(EnemyArchetype.Boss, count);

    public bool SpawnEventEnemies(EnemyArchetype archetype, int count) => SpawnEventEnemies(archetype, count, null);

    public bool SpawnEventEnemies(EnemyArchetype archetype, int count, Vector2? spawnOriginOverride)
    {
        if (count <= 0)
        {
            return false;
        }

        System.Func<PackEntry, float> getWeight = entry => EnemySpawnEntryUtility.GetEventSpawnWeight(entry, archetype);

        if (!EnemySpawnEntryUtility.HasWeightedEntries(eventPacks, getWeight))
        {
            Debug.LogError(EnemySpawnEntryUtility.GetMissingEventSpawnReason(eventPacks, archetype));
            return false;
        }

        int spawnedCount = 0;

        for (int i = 0; i < count; i++)
        {
            PackEntry eventEntry = EnemySpawnEntryUtility.RollWeightedEntry(eventPacks, getWeight);
            if (eventEntry == null)
            {
                break;
            }

            SpawnPack(eventEntry, BuildSpawnContext(archetype, EnemyRarity.Normal), spawnOriginOverride);
            spawnedCount++;

            if (archetype == EnemyArchetype.Miniboss && Random.value < GetAtlasPercentModifier(AtlasEffectType.DoubleMiniBossSpawnChancePercent))
            {
                SpawnPack(eventEntry, BuildSpawnContext(archetype, EnemyRarity.Normal), spawnOriginOverride);
            }
        }

        return spawnedCount == count;
    }

    public void AddRuntimeModifier(EnemySpawnerModifierType modifierType, float additiveValue, float durationSeconds = 0f)
    {
        if (durationSeconds > 0f)
        {
            temporaryRuntimeModifiers.Add(new RuntimeSpawnerModifier
            {
                type = modifierType,
                additiveValue = additiveValue,
                remainingDuration = durationSeconds,
            });
            return;
        }

        persistentRuntimeModifiers.TryGetValue(modifierType, out float currentValue);
        persistentRuntimeModifiers[modifierType] = currentValue + additiveValue;
    }

    private void SpawnPack(PackEntry entry, EnemySpawnContext? packContextOverride = null, Vector2? packOriginOverride = null)
    {
        if (entry == null
            || entry.enemyPrefab == null
            || !EnemySpawnEntryUtility.TryGetPackDefinition(entry, out Enemy enemyTemplate, out _))
        {
            return;
        }

        EnemySpawnContext packContext = packContextOverride ?? BuildSpawnContext(enemyTemplate.Archetype);
        int packSize = GetPackSize(enemyTemplate);
        // Ambient packs choose a chunk-aware origin, while event spawns can override it.
        Vector2? generatedPackOrigin = packOriginOverride ?? GetAmbientPackSpawnOrigin();
        if (!generatedPackOrigin.HasValue)
        {
            return;
        }

        Vector2 packOrigin = generatedPackOrigin.Value;

        for (int i = 0; i < packSize; i++)
        {
            SpawnEnemy(entry.enemyPrefab, packOrigin, packContext);
        }
    }

    private int GetPackSize(Enemy enemyTemplate)
    {
        if (enemyTemplate == null)
        {
            return 1;
        }

        int packSize = enemyTemplate.RollPackSize(GetEffectiveModifier(EnemySpawnerModifierType.EnemyQuality));
        return Mathf.Max(1, Mathf.RoundToInt(packSize * Mathf.Max(0f, GetEffectiveModifier(EnemySpawnerModifierType.EnemyQuantity))));
    }

    private void SpawnEnemy(GameObject enemyPrefab, Vector2 packOrigin, EnemySpawnContext packContext)
    {
        Vector2 spawnPoint = GetPackSpawnPoint(packOrigin);
        Enemy enemy = GetPooledEnemy(enemyPrefab);

        if (enemy != null)
        {
            enemy.transform.SetPositionAndRotation(spawnPoint, transform.rotation);
            enemy.ConfigurePool(this, enemyPrefab);
            enemy.Initialize(packContext);
        }
    }

    public void ReturnEnemyToPool(Enemy enemy, GameObject prefabKey)
    {
        if (enemy == null || prefabKey == null)
        {
            return;
        }

        if (!TryGetEnemyPools(out EnemyPools pools))
        {
            Destroy(enemy.gameObject);
            return;
        }

        pools.ReturnEnemy(enemy, prefabKey);
    }

    private EnemySpawnContext BuildSpawnContext(EnemyRarity? rarityOverride = null)
    {
        return BuildSpawnContext(EnemyArchetype.Fodder, rarityOverride);
    }

    private EnemySpawnContext BuildSpawnContext(EnemyArchetype archetype, EnemyRarity? rarityOverride = null)
    {
        EnemyRarity rarity = rarityOverride ?? RollRarity();
        RarityProfile rarityProfile = GetRarityProfile(rarity);

        return new EnemySpawnContext
        {
            rarity = rarity,
            healthMultiplier = rarityProfile.healthMultiplier
                * GetEffectiveModifier(EnemySpawnerModifierType.EnemyHealth)
                * GetArchetypeHealthMultiplier(archetype)
                * GetBossHealthMultiplier(archetype),
            damageMultiplier = rarityProfile.damageMultiplier
                * GetEffectiveModifier(EnemySpawnerModifierType.EnemyDamage)
                * GetArchetypeDamageMultiplier(archetype)
                * GetBossDamageMultiplier(archetype),
            moveSpeedMultiplier = rarityProfile.moveSpeedMultiplier
                * GetEffectiveModifier(EnemySpawnerModifierType.EnemyMoveSpeed)
                * GetArchetypeMoveSpeedMultiplier(archetype),
            experienceMultiplier = rarityProfile.experienceMultiplier
                * GetEffectiveModifier(EnemySpawnerModifierType.ExperienceWorth)
                * GetBossExperienceMultiplier(archetype),
            dropChanceMultiplier = GetEffectiveModifier(EnemySpawnerModifierType.DropChance),
        };
    }

    private EnemyRarity RollRarity()
    {
        float quality = Mathf.Max(0f, GetEffectiveModifier(EnemySpawnerModifierType.EnemyQuality));
        float rareChance = Mathf.Clamp01(RareChancePerQuality * quality);
        float uncommonChance = Mathf.Clamp01(UncommonChancePerQuality * quality);
        ApplyAtlasEnemyQuality(ref uncommonChance, ref rareChance);
        float roll = Random.value;

        if (roll < rareChance)
        {
            return EnemyRarity.Rare;
        }

        if (roll < rareChance + uncommonChance)
        {
            return EnemyRarity.Uncommon;
        }

        return EnemyRarity.Normal;
    }

    private RarityProfile GetRarityProfile(EnemyRarity rarity)
    {
        return rarity switch
        {
            EnemyRarity.Uncommon => UncommonRarityProfile,
            EnemyRarity.Rare => RareRarityProfile,
            _ => NormalRarityProfile,
        };
    }

    private void ApplyRunModifiers()
    {
        mapModifiers = BuildMapSpawnModifiers();
        // Mirrors the resolved map modifiers into the legacy debug fields still shown in the inspector.
        modifiers.quantity = mapModifiers.quantity;
        modifiers.quality = mapModifiers.quality;
    }

    private float GetEffectiveModifier(EnemySpawnerModifierType modifierType)
    {
        return Mathf.Max(0f, GetBaseModifier(modifierType) + GetRuntimeModifierSum(modifierType));
    }

    private float GetBaseModifier(EnemySpawnerModifierType modifierType)
    {
        return modifierType switch
        {
            EnemySpawnerModifierType.EnemyQuantity => mapModifiers.quantity,
            EnemySpawnerModifierType.EnemyQuality => mapModifiers.quality,
            EnemySpawnerModifierType.EnemyDamage => mapModifiers.damage,
            EnemySpawnerModifierType.EnemyHealth => mapModifiers.health,
            EnemySpawnerModifierType.EnemyMoveSpeed => mapModifiers.moveSpeed,
            EnemySpawnerModifierType.ExperienceWorth => mapModifiers.experience,
            EnemySpawnerModifierType.DropChance => mapModifiers.dropChance,
            EnemySpawnerModifierType.PackSpawnInterval => 1f,
            _ => 1f,
        };
    }

    private float GetRuntimeModifierSum(EnemySpawnerModifierType modifierType)
    {
        float total = 0f;

        if (persistentRuntimeModifiers.TryGetValue(modifierType, out float persistentValue))
        {
            total += persistentValue;
        }

        for (int i = 0; i < temporaryRuntimeModifiers.Count; i++)
        {
            RuntimeSpawnerModifier modifier = temporaryRuntimeModifiers[i];
            if (modifier.type == modifierType)
            {
                total += modifier.additiveValue;
            }
        }

        return total;
    }

    private void UpdateTemporaryModifiers()
    {
        for (int i = temporaryRuntimeModifiers.Count - 1; i >= 0; i--)
        {
            RuntimeSpawnerModifier modifier = temporaryRuntimeModifiers[i];
            modifier.remainingDuration -= Time.deltaTime;

            if (modifier.remainingDuration <= 0f)
            {
                temporaryRuntimeModifiers.RemoveAt(i);
                continue;
            }

            temporaryRuntimeModifiers[i] = modifier;
        }
    }

    private MapSpawnModifiers BuildMapSpawnModifiers()
    {
        EquipmentStatSummary equipmentSummary = MetaProgressionService.GetEquippedEquipmentStatSummary();

        return new MapSpawnModifiers
        {
            quantity = BuildMapModifier(
                MapStatType.EnemyQuantity,
                equipmentSummary,
                EquipmentStatType.EnemyQuantity,
                AtlasEffectType.EnemyQuantityPercent),
            quality = BuildMapModifier(MapStatType.EnemyQuality, equipmentSummary, EquipmentStatType.EnemyQuality),
            damage = BuildMapModifier(
                MapStatType.EnemyDamage,
                atlasEffectType: AtlasEffectType.EnemyDamagePercent),
            health = BuildMapModifier(
                MapStatType.EnemyHealth,
                atlasEffectType: AtlasEffectType.EnemyLifePercent),
            moveSpeed = BuildMapModifier(MapStatType.EnemyMoveSpeed),
            experience = BuildMapModifier(MapStatType.ExperienceWorth),
            dropChance = BuildMapModifier(MapStatType.DropChance),
        };
    }

    private float BuildMapModifier(
        MapStatType mapStatType,
        EquipmentStatSummary equipmentSummary = null,
        EquipmentStatType? equipmentStatType = null,
        AtlasEffectType? atlasEffectType = null)
    {
        float modifier = 1f + GetMapModifier(mapStatType);

        if (equipmentSummary != null && equipmentStatType.HasValue)
        {
            modifier += GetEquipmentPercentModifier(equipmentSummary, equipmentStatType.Value);
        }

        if (atlasEffectType.HasValue)
        {
            modifier += GetAtlasPercentModifier(atlasEffectType.Value);
        }

        return modifier;
    }

    private static float GetEquipmentPercentModifier(EquipmentStatSummary summary, EquipmentStatType statType) => summary?.GetEntry(statType)?.percentValue ?? 0f;

    private static float GetMapModifier(MapStatType statType) => RunData.SelectedMap == null ? 0f : RunData.SelectedMap.GetModifier(statType) / 100f;

    private static float GetAtlasPercentModifier(AtlasEffectType effectType) =>
        MetaProgressionService.GetAtlasEffectValue(effectType) / 100f;

    private static void ApplyAtlasEnemyQuality(ref float uncommonChance, ref float rareChance)
    {
        float normalChance = Mathf.Clamp01(1f - uncommonChance - rareChance);
        float qualityTransfer = Mathf.Min(normalChance, Mathf.Max(0f, GetAtlasPercentModifier(AtlasEffectType.EnemyQualityPercent)));

        uncommonChance = Mathf.Clamp01(uncommonChance + (qualityTransfer * 0.5f));
        rareChance = Mathf.Clamp01(rareChance + (qualityTransfer * 0.5f));
    }

    private static float GetArchetypeSpawnWeightMultiplier(EnemyArchetype archetype) =>
        archetype switch
        {
            EnemyArchetype.Elite => 1f + GetAtlasPercentModifier(AtlasEffectType.EliteVariantChancePercent),
            EnemyArchetype.Tank => 1f + GetAtlasPercentModifier(AtlasEffectType.TankVariantChancePercent),
            EnemyArchetype.Skirmisher => 1f + GetAtlasPercentModifier(AtlasEffectType.SkirmisherVariantChancePercent),
            _ => 1f,
        };

    private static float GetArchetypeHealthMultiplier(EnemyArchetype archetype) =>
        archetype switch
        {
            EnemyArchetype.Elite => 1f + GetAtlasPercentModifier(AtlasEffectType.EliteEnemyHealthPercent),
            EnemyArchetype.Tank => 1f + GetAtlasPercentModifier(AtlasEffectType.TankEnemyHealthPercent),
            _ => 1f,
        };

    private static float GetArchetypeDamageMultiplier(EnemyArchetype archetype) =>
        archetype == EnemyArchetype.Elite
            ? 1f + GetAtlasPercentModifier(AtlasEffectType.EliteEnemyDamagePercent)
            : 1f;

    private static float GetArchetypeMoveSpeedMultiplier(EnemyArchetype archetype) =>
        archetype == EnemyArchetype.Skirmisher
            ? 1f + GetAtlasPercentModifier(AtlasEffectType.SkirmisherEnemyMoveSpeedPercent)
            : 1f;

    private static bool IsBossArchetype(EnemyArchetype archetype) =>
        archetype == EnemyArchetype.Boss || archetype == EnemyArchetype.Miniboss;

    private static float GetBossExperienceMultiplier(EnemyArchetype archetype) =>
        IsBossArchetype(archetype)
            ? 1f + GetAtlasPercentModifier(AtlasEffectType.BossExperiencePercent)
            : 1f;

    private static float GetBossHealthMultiplier(EnemyArchetype archetype) =>
        IsBossArchetype(archetype)
            ? 1f + GetAtlasPercentModifier(AtlasEffectType.BossLifePercent)
            : 1f;

    private static float GetBossDamageMultiplier(EnemyArchetype archetype) =>
        IsBossArchetype(archetype)
            ? 1f + GetAtlasPercentModifier(AtlasEffectType.BossDamagePercent)
            : 1f;

    private Vector2? GetAmbientPackSpawnOrigin()
    {
        if (!TryGetWorldChunkManager(out WorldChunkManager chunkManager) || player == null)
        {
            return null;
        }

        float minimumDistance = Mathf.Max(0f, ambientSpawnMinimumWorldDistance);
        float maximumDistance = Mathf.Max(minimumDistance, ambientSpawnMaximumWorldDistance);
        int attemptCount = Mathf.Max(1, ambientSpawnMaxAttempts);

        for (int i = 0; i < attemptCount; i++)
        {
            Vector2 candidatePoint = player.transform.position
                + (Vector3)(Random.insideUnitCircle.normalized * Random.Range(minimumDistance, maximumDistance));
            ChunkCoordinate candidateChunk = ChunkWorldUtility.GetChunkCoordinate(
                candidatePoint,
                chunkManager.ChunkSizeTiles,
                chunkManager.TileSize);

            if (chunkManager.IsChunkLoaded(candidateChunk))
            {
                return candidatePoint;
            }
        }

        return null;
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

    private bool TryGetEnemyPools(out EnemyPools pools)
    {
        if (enemyPools == null)
        {
            enemyPools = EnemyPools.Instance ?? FindAnyObjectByType<EnemyPools>();

            if (enemyPools == null)
            {
                // Older scenes may miss the pool bootstrap, so the spawner creates a temporary pool root on demand.
                GameObject poolsObject = new GameObject("EnemyPools");
                enemyPools = poolsObject.AddComponent<EnemyPools>();
            }
        }

        pools = enemyPools;
        return pools != null;
    }

    private Vector2 GetPackSpawnPoint(Vector2 packOrigin) => packSpawnRadius <= 0f ? packOrigin : packOrigin + (Random.insideUnitCircle * packSpawnRadius);

    private void BuildEnemyPools()
    {
        if (!TryGetEnemyPools(out EnemyPools pools))
        {
            return;
        }

        HashSet<GameObject> uniquePrefabs = new HashSet<GameObject>();
        EnemySpawnEntryUtility.CollectPoolPrefabs(ambientPacks, uniquePrefabs);
        EnemySpawnEntryUtility.CollectPoolPrefabs(eventPacks, uniquePrefabs);
        pools.EnsurePools(new List<GameObject>(uniquePrefabs), initialPoolSizePerEnemy);
    }

    private Enemy GetPooledEnemy(GameObject enemyPrefab) =>
        enemyPrefab != null && TryGetEnemyPools(out EnemyPools pools)
            ? pools.GetEnemy(enemyPrefab, initialPoolSizePerEnemy)
            : null;

    private static RarityProfile CreateRarityProfile(
        float healthMultiplier,
        float damageMultiplier,
        float moveSpeedMultiplier,
        float experienceMultiplier)
    {
        return new RarityProfile
        {
            healthMultiplier = healthMultiplier,
            damageMultiplier = damageMultiplier,
            moveSpeedMultiplier = moveSpeedMultiplier,
            experienceMultiplier = experienceMultiplier,
        };
    }
}
