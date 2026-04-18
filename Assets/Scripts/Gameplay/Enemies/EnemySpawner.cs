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
        spawnTimer = 0f;
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
        if (player == null)
        {
            player = PlayerController.Instance;
        }

        return player != null
            && player.gameObject.activeSelf
            && HasAmbientSpawnEntries()
            && TryGetWorldChunkManager(out _);
    }

    private bool HasAmbientSpawnEntries()
    {
        for (int i = 0; i < ambientPacks.Count; i++)
        {
            if (GetAmbientSpawnWeight(ambientPacks[i]) > 0f)
            {
                return true;
            }
        }

        return false;
    }

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
        float intervalMultiplier = Mathf.Max(0.1f, 1f + GetRuntimeModifierSum(EnemySpawnerModifierType.PackSpawnInterval));
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

    private PackEntry RollAmbientSpawnEntry()
    {
        return RollSpawnEntry(GetAmbientSpawnWeight);
    }

    private float GetAmbientSpawnWeight(PackEntry entry)
    {
        if (entry == null || entry.enemyPrefab == null)
        {
            return 0f;
        }

        Enemy enemyTemplate = entry.enemyPrefab.GetComponent<Enemy>();
        EnemyArchetypeDefinition archetypeDefinition = enemyTemplate != null ? enemyTemplate.ArchetypeDefinition : null;

        return archetypeDefinition != null ? archetypeDefinition.AmbientSpawnWeight : 0f;
    }

    public bool SpawnFinalBosses(int count)
    {
        return SpawnEventEnemies(EnemyArchetype.Boss, count);
    }

    public bool SpawnEventEnemies(EnemyArchetype archetype, int count)
    {
        return SpawnEventEnemies(archetype, count, null);
    }

    public bool SpawnEventEnemies(EnemyArchetype archetype, int count, Vector2? spawnOriginOverride)
    {
        if (count <= 0)
        {
            return false;
        }

        if (!HasEventEntries(archetype))
        {
            Debug.LogError(GetMissingEventSpawnReason(archetype));
            return false;
        }

        int spawnedCount = 0;

        for (int i = 0; i < count; i++)
        {
            PackEntry eventEntry = RollSpawnEntry(eventPacks, entry => GetEventSpawnWeight(entry, archetype));
            if (eventEntry == null)
            {
                break;
            }

            SpawnPack(eventEntry, BuildEventSpawnContext(), spawnOriginOverride);
            spawnedCount++;
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

    private bool HasEventEntries(EnemyArchetype archetype)
    {
        for (int i = 0; i < eventPacks.Count; i++)
        {
            if (GetEventSpawnWeight(eventPacks[i], archetype) > 0f)
            {
                return true;
            }
        }

        return false;
    }

    private string GetMissingEventSpawnReason(EnemyArchetype archetype)
    {
        if (eventPacks.Count == 0)
        {
            return $"Event spawn for '{archetype}' could not start because the Event Spawn Pool is empty.";
        }

        for (int i = 0; i < eventPacks.Count; i++)
        {
            PackEntry entry = eventPacks[i];

            if (entry == null || entry.enemyPrefab == null)
            {
                continue;
            }

            Enemy enemyTemplate = entry.enemyPrefab.GetComponent<Enemy>();
            if (enemyTemplate == null)
            {
                return $"Event spawn for '{archetype}' could not start because event pack '{entry.enemyPrefab.name}' does not have an Enemy component.";
            }

            EnemyArchetypeDefinition archetypeDefinition = enemyTemplate.ArchetypeDefinition;
            if (archetypeDefinition == null)
            {
                return $"Event spawn for '{archetype}' could not start because event pack '{entry.enemyPrefab.name}' has no archetype definition assigned.";
            }

            if (archetypeDefinition.SpawnRole != EnemySpawnRole.EventOnly)
            {
                return $"Event spawn for '{archetype}' could not start because event pack '{entry.enemyPrefab.name}' is not marked EventOnly.";
            }

            if (archetypeDefinition.Archetype != archetype)
            {
                return $"Event spawn for '{archetype}' could not start because event pack '{entry.enemyPrefab.name}' is '{archetypeDefinition.Archetype}' instead of '{archetype}'.";
            }
        }

        return $"Event spawn for '{archetype}' could not start because no valid {archetype} + EventOnly entry was found in the Event Spawn Pool.";
    }

    private float GetEventSpawnWeight(PackEntry entry, EnemyArchetype archetype)
    {
        if (entry == null || entry.enemyPrefab == null)
        {
            return 0f;
        }

        Enemy enemyTemplate = entry.enemyPrefab.GetComponent<Enemy>();
        EnemyArchetypeDefinition archetypeDefinition = enemyTemplate != null ? enemyTemplate.ArchetypeDefinition : null;

        if (archetypeDefinition == null
            || archetypeDefinition.SpawnRole != EnemySpawnRole.EventOnly
            || archetypeDefinition.Archetype != archetype)
        {
            return 0f;
        }

        return 1f;
    }

    private PackEntry RollSpawnEntry(System.Func<PackEntry, float> getWeight)
    {
        return RollSpawnEntry(ambientPacks, getWeight);
    }

    private PackEntry RollSpawnEntry(IReadOnlyList<PackEntry> sourceEntries, System.Func<PackEntry, float> getWeight)
    {
        if (sourceEntries == null || getWeight == null)
        {
            return null;
        }

        float totalWeight = 0f;

        for (int i = 0; i < sourceEntries.Count; i++)
        {
            totalWeight += getWeight(sourceEntries[i]);
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float roll = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < sourceEntries.Count; i++)
        {
            PackEntry entry = sourceEntries[i];
            currentWeight += getWeight(entry);

            if (roll <= currentWeight)
            {
                return entry;
            }
        }

        return null;
    }

    private void SpawnPack(PackEntry entry)
    {
        SpawnPack(entry, BuildSpawnContext());
    }

    private void SpawnPack(PackEntry entry, EnemySpawnContext packContext)
    {
        SpawnPack(entry, packContext, null);
    }

    private void SpawnPack(PackEntry entry, EnemySpawnContext packContext, Vector2? packOriginOverride)
    {
        if (entry == null || entry.enemyPrefab == null)
        {
            return;
        }

        Enemy enemyTemplate = entry.enemyPrefab.GetComponent<Enemy>();
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

    private EnemySpawnContext BuildSpawnContext()
    {
        EnemyRarity rarity = RollRarity();
        RarityProfile rarityProfile = GetRarityProfile(rarity);

        return new EnemySpawnContext
        {
            rarity = rarity,
            healthMultiplier = rarityProfile.healthMultiplier * GetEffectiveModifier(EnemySpawnerModifierType.EnemyHealth),
            damageMultiplier = rarityProfile.damageMultiplier * GetEffectiveModifier(EnemySpawnerModifierType.EnemyDamage),
            moveSpeedMultiplier = rarityProfile.moveSpeedMultiplier * GetEffectiveModifier(EnemySpawnerModifierType.EnemyMoveSpeed),
            experienceMultiplier = rarityProfile.experienceMultiplier * GetEffectiveModifier(EnemySpawnerModifierType.ExperienceWorth),
            dropChanceMultiplier = GetEffectiveModifier(EnemySpawnerModifierType.DropChance),
        };
    }

    private EnemySpawnContext BuildEventSpawnContext()
    {
        return new EnemySpawnContext
        {
            rarity = EnemyRarity.Normal,
            healthMultiplier = GetEffectiveModifier(EnemySpawnerModifierType.EnemyHealth),
            damageMultiplier = GetEffectiveModifier(EnemySpawnerModifierType.EnemyDamage),
            moveSpeedMultiplier = GetEffectiveModifier(EnemySpawnerModifierType.EnemyMoveSpeed),
            experienceMultiplier = GetEffectiveModifier(EnemySpawnerModifierType.ExperienceWorth),
            dropChanceMultiplier = GetEffectiveModifier(EnemySpawnerModifierType.DropChance),
        };
    }

    private EnemyRarity RollRarity()
    {
        float quality = Mathf.Max(0f, GetEffectiveModifier(EnemySpawnerModifierType.EnemyQuality));
        float rareChance = Mathf.Clamp01(0.03f * quality);
        float uncommonChance = Mathf.Clamp01(0.12f * quality);
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
        switch (rarity)
        {
            case EnemyRarity.Uncommon:
                return new RarityProfile
                {
                    healthMultiplier = 1.5f,
                    damageMultiplier = 1.2f,
                    moveSpeedMultiplier = 1.05f,
                    experienceMultiplier = 1.5f,
                };

            case EnemyRarity.Rare:
                return new RarityProfile
                {
                    healthMultiplier = 2.5f,
                    damageMultiplier = 1.5f,
                    moveSpeedMultiplier = 1.1f,
                    experienceMultiplier = 2.5f,
                };

            default:
                return new RarityProfile
                {
                    healthMultiplier = 1f,
                    damageMultiplier = 1f,
                    moveSpeedMultiplier = 1f,
                    experienceMultiplier = 1f,
                };
        }
    }

    private void ApplyRunModifiers()
    {
        mapModifiers = BuildMapSpawnModifiers();
        modifiers.quantity = mapModifiers.quantity;
        modifiers.quality = mapModifiers.quality;
    }

    private float GetEffectiveModifier(EnemySpawnerModifierType modifierType)
    {
        float baseValue = modifierType switch
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

        return Mathf.Max(0f, baseValue + GetRuntimeModifierSum(modifierType));
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
            quantity = 1f + GetMapModifier(MapStatType.EnemyQuantity) + GetEquipmentPercentModifier(equipmentSummary, EquipmentStatType.EnemyQuantity),
            quality = 1f + GetMapModifier(MapStatType.EnemyQuality) + GetEquipmentPercentModifier(equipmentSummary, EquipmentStatType.EnemyQuality),
            damage = 1f + GetMapModifier(MapStatType.EnemyDamage),
            health = 1f + GetMapModifier(MapStatType.EnemyHealth),
            moveSpeed = 1f + GetMapModifier(MapStatType.EnemyMoveSpeed),
            experience = 1f + GetMapModifier(MapStatType.ExperienceWorth),
            dropChance = 1f + GetMapModifier(MapStatType.DropChance),
        };
    }

    private float GetEquipmentPercentModifier(EquipmentStatSummary summary, EquipmentStatType statType)
    {
        return summary?.GetEntry(statType)?.percentValue ?? 0f;
    }

    private float GetMapModifier(MapStatType statType)
    {
        if (RunData.SelectedMap == null)
        {
            return 0f;
        }

        return RunData.SelectedMap.GetModifier(statType) / 100f;
    }

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
                GameObject poolsObject = new GameObject("EnemyPools");
                enemyPools = poolsObject.AddComponent<EnemyPools>();
            }
        }

        pools = enemyPools;
        return pools != null;
    }

    private Vector2 GetPackSpawnPoint(Vector2 packOrigin)
    {
        if (packSpawnRadius <= 0f)
        {
            return packOrigin;
        }

        Vector2 offset = Random.insideUnitCircle * packSpawnRadius;
        return packOrigin + offset;
    }

    private void BuildEnemyPools()
    {
        if (!TryGetEnemyPools(out EnemyPools pools))
        {
            return;
        }

        HashSet<GameObject> uniquePrefabs = new HashSet<GameObject>();
        CollectPoolPrefabs(ambientPacks, uniquePrefabs);
        CollectPoolPrefabs(eventPacks, uniquePrefabs);
        pools.EnsurePools(new List<GameObject>(uniquePrefabs), initialPoolSizePerEnemy);
    }

    private Enemy GetPooledEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null || !TryGetEnemyPools(out EnemyPools pools))
        {
            return null;
        }

        return pools.GetEnemy(enemyPrefab, initialPoolSizePerEnemy);
    }

    private void CollectPoolPrefabs(IReadOnlyList<PackEntry> entries, ISet<GameObject> output)
    {
        if (entries == null || output == null)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            PackEntry entry = entries[i];
            if (entry == null || entry.enemyPrefab == null)
            {
                continue;
            }

            output.Add(entry.enemyPrefab);
        }
    }
}
