using System.Collections.Generic;
using UnityEngine;

// Spawns ambient enemy packs over time using archetype weights and map progress scaling.
public class EnemySpawner : MonoBehaviour
{
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
    public class Wave
    {
        // Legacy list entry kept intentionally so existing inspector prefab assignments survive.
        public GameObject enemyPrefab;
    }

    [Header("Ambient Spawn Pool")]
    public List<Wave> waves = new List<Wave>();

    [Header("Spawn Bounds")]
    [SerializeField] private Transform minPos;
    [SerializeField] private Transform maxPos;

    [Header("Run Modifiers")]
    [SerializeField] private EnemySpawnModifiers modifiers = new EnemySpawnModifiers();

    [Header("Pack Spawning")]
    [SerializeField, Min(0.1f)] private float basePackSpawnInterval = 5f;
    [SerializeField, Min(0f)] private float spawnIntervalReductionPerQuarter = 0.5f;
    [SerializeField, Min(0.1f)] private float minimumPackSpawnInterval = 2f;
    [SerializeField, Min(0f)] private float packSpawnRadius = 1.25f;

    private MapSpawnModifiers mapModifiers;
    private PlayerController player;
    private float spawnTimer;

    void Awake()
    {
        ApplyRunModifiers();
        spawnTimer = 0f;
    }

    void Update()
    {
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
            Wave selectedEntry = RollAmbientSpawnEntry();
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

        return player != null && player.gameObject.activeSelf && HasAmbientSpawnEntries();
    }

    private bool HasAmbientSpawnEntries()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            if (GetAmbientSpawnWeight(waves[i]) > 0f)
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
        return Mathf.Max(minimumPackSpawnInterval, basePackSpawnInterval - intervalReduction);
    }

    private float GetVictoryCompletionRate()
    {
        if (RunData.SelectedMap == null || GameManager.Instance == null || RunData.SelectedMap.VictoryTarget <= 0)
        {
            return 0f;
        }

        return RunData.SelectedMap.VictoryConditionType switch
        {
            VictoryConditionType.Time => Mathf.Clamp01(GameManager.Instance.gameTime / (RunData.SelectedMap.VictoryTarget * 60f)),
            VictoryConditionType.Kills => Mathf.Clamp01((float)GameManager.Instance.enemyKills / RunData.SelectedMap.VictoryTarget),
            _ => 0f,
        };
    }

    private int GetPackCountPerSpawn()
    {
        float quantityMultiplier = Mathf.Max(0f, modifiers.quantity);
        int guaranteedPacks = Mathf.FloorToInt(quantityMultiplier);
        float fractionalPackChance = quantityMultiplier - guaranteedPacks;
        int packCount = guaranteedPacks;

        if (Random.value < fractionalPackChance)
        {
            packCount++;
        }

        return Mathf.Max(1, packCount);
    }

    private Wave RollAmbientSpawnEntry()
    {
        float totalWeight = 0f;

        for (int i = 0; i < waves.Count; i++)
        {
            totalWeight += GetAmbientSpawnWeight(waves[i]);
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float roll = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < waves.Count; i++)
        {
            Wave entry = waves[i];
            currentWeight += GetAmbientSpawnWeight(entry);

            if (roll <= currentWeight)
            {
                return entry;
            }
        }

        return null;
    }

    private float GetAmbientSpawnWeight(Wave entry)
    {
        if (entry == null || entry.enemyPrefab == null)
        {
            return 0f;
        }

        Enemy enemyTemplate = entry.enemyPrefab.GetComponent<Enemy>();
        EnemyArchetypeDefinition archetypeDefinition = enemyTemplate != null ? enemyTemplate.ArchetypeDefinition : null;

        return archetypeDefinition != null ? archetypeDefinition.AmbientSpawnWeight : 0f;
    }

    private void SpawnPack(Wave entry)
    {
        if (entry == null || entry.enemyPrefab == null)
        {
            return;
        }

        Enemy enemyTemplate = entry.enemyPrefab.GetComponent<Enemy>();
        int packSize = GetPackSize(enemyTemplate);
        EnemySpawnContext packContext = BuildSpawnContext();
        Vector2 packOrigin = GetSpawnPoint();

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

        int packSize = enemyTemplate.RollPackSize(mapModifiers.quality);
        return Mathf.Max(1, Mathf.RoundToInt(packSize * Mathf.Max(0f, modifiers.quantity)));
    }

    private void SpawnEnemy(GameObject enemyPrefab, Vector2 packOrigin, EnemySpawnContext packContext)
    {
        Vector2 spawnPoint = GetPackSpawnPoint(packOrigin);
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPoint, transform.rotation);
        Enemy enemy = enemyObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.Initialize(packContext);
        }
    }

    private EnemySpawnContext BuildSpawnContext()
    {
        EnemyRarity rarity = RollRarity();
        RarityProfile rarityProfile = GetRarityProfile(rarity);

        return new EnemySpawnContext
        {
            rarity = rarity,
            healthMultiplier = rarityProfile.healthMultiplier * mapModifiers.health,
            damageMultiplier = rarityProfile.damageMultiplier * mapModifiers.damage,
            moveSpeedMultiplier = rarityProfile.moveSpeedMultiplier * mapModifiers.moveSpeed,
            experienceMultiplier = rarityProfile.experienceMultiplier * mapModifiers.experience,
            dropChanceMultiplier = mapModifiers.dropChance,
        };
    }

    private EnemyRarity RollRarity()
    {
        float quality = Mathf.Max(0f, modifiers.quality);
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

    private MapSpawnModifiers BuildMapSpawnModifiers()
    {
        return new MapSpawnModifiers
        {
            quantity = 1f + GetMapModifier(MapStatType.EnemyQuantity),
            quality = 1f + GetMapModifier(MapStatType.EnemyQuality),
            damage = 1f + GetMapModifier(MapStatType.EnemyDamage),
            health = 1f + GetMapModifier(MapStatType.EnemyHealth),
            moveSpeed = 1f + GetMapModifier(MapStatType.EnemyMoveSpeed),
            experience = 1f + GetMapModifier(MapStatType.ExperienceWorth),
            dropChance = 1f + GetMapModifier(MapStatType.DropChance),
        };
    }

    private float GetMapModifier(MapStatType statType)
    {
        if (RunData.SelectedMap == null)
        {
            return 0f;
        }

        return RunData.SelectedMap.GetModifier(statType) / 100f;
    }

    private Vector2 GetSpawnPoint()
    {
        Vector2 spawnPoint;

        if (Random.Range(0f, 1f) > 0.5)
        {
            spawnPoint.x = Random.Range(minPos.position.x, maxPos.position.x);

            if (Random.Range(0f, 1f) > 0.5)
            {
                spawnPoint.y = minPos.position.y;
            }
            else
            {
                spawnPoint.y = maxPos.position.y;
            }
        }
        else
        {
            spawnPoint.y = Random.Range(minPos.position.y, maxPos.position.y);

            if (Random.Range(0f, 1f) > 0.5)
            {
                spawnPoint.x = minPos.position.x;
            }
            else
            {
                spawnPoint.x = maxPos.position.x;
            }
        }

        return spawnPoint;
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
}
