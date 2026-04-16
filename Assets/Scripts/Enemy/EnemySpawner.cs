using System.Collections.Generic;
using UnityEngine;

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
        public GameObject enemyPrefab;
        public float spawnInterval = 1f;
        public int enemiesPerWave = 10;
        public float minimumSpawnInterval = 0.15f;
        [Range(0.1f, 1f)] public float spawnIntervalDecay = 0.8f;
    }

    private class SpawnRuntimeState
    {
        public float spawnTimer;
        public float currentSpawnInterval;
        public int completedSpawnCycles;
    }

    [Header("Spawn Sequence")]
    public List<Wave> waves = new List<Wave>();

    [Header("Spawn Bounds")]
    [SerializeField] private Transform minPos;
    [SerializeField] private Transform maxPos;

    [Header("Run Modifiers")]
    [SerializeField] private EnemySpawnModifiers modifiers = new EnemySpawnModifiers();

    [Header("Pack Spawning")]
    [SerializeField, Min(0f)] private float packSpawnRadius = 1.25f;

    private readonly List<SpawnRuntimeState> runtimeStates = new List<SpawnRuntimeState>();
    private MapSpawnModifiers mapModifiers;
    public int waveNumber;
    private PlayerController player;

    void Awake()
    {
        ApplyRunModifiers();
        BuildRuntimeStates();
    }

    void Update()
    {
        if (!CanRunSpawner())
        {
            return;
        }

        Wave currentWave = waves[waveNumber];
        SpawnRuntimeState currentState = runtimeStates[waveNumber];

        if (!ShouldSpawn(currentState))
        {
            return;
        }

        SpawnPack(currentWave);
        currentState.completedSpawnCycles++;

        if (currentState.completedSpawnCycles < currentWave.enemiesPerWave)
        {
            return;
        }

        AdvanceToNextWave(currentWave, currentState);
    }

    private bool CanRunSpawner()
    {
        if (player == null)
        {
            player = PlayerController.Instance;
        }

        return player != null && player.gameObject.activeSelf && waves.Count > 0;
    }

    private bool ShouldSpawn(SpawnRuntimeState currentState)
    {
        currentState.spawnTimer += Time.deltaTime;

        if (currentState.spawnTimer < currentState.currentSpawnInterval)
        {
            return false;
        }

        currentState.spawnTimer = 0f;
        return true;
    }

    private void BuildRuntimeStates()
    {
        runtimeStates.Clear();

        foreach (Wave wave in waves)
        {
            runtimeStates.Add(new SpawnRuntimeState
            {
                spawnTimer = 0f,
                currentSpawnInterval = wave.spawnInterval,
                completedSpawnCycles = 0,
            });
        }
    }

    private void SpawnPack(Wave wave)
    {
        if (wave == null || wave.enemyPrefab == null)
        {
            return;
        }

        Enemy enemyTemplate = wave.enemyPrefab.GetComponent<Enemy>();
        int packSize = GetPackSize(enemyTemplate);
        EnemySpawnContext packContext = BuildSpawnContext();
        Vector2 packOrigin = GetSpawnPoint();

        for (int i = 0; i < packSize; i++)
        {
            SpawnEnemy(wave.enemyPrefab, packOrigin, packContext);
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

    private void AdvanceToNextWave(Wave wave, SpawnRuntimeState currentState)
    {
        currentState.completedSpawnCycles = 0;
        currentState.currentSpawnInterval = Mathf.Max(
            wave.minimumSpawnInterval,
            currentState.currentSpawnInterval * wave.spawnIntervalDecay
        );

        waveNumber++;

        if (waveNumber >= waves.Count)
        {
            waveNumber = 0;
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
