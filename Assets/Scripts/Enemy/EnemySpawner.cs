using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject enemyPrefab;
        public float spawnInterval = 1f;
        public int enemiesPerWave = 10;
        public int packSize = 1;
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

    private readonly List<SpawnRuntimeState> runtimeStates = new List<SpawnRuntimeState>();
    public int waveNumber;
    private PlayerController player;

    void Awake()
    {
        ApplyRunModifiers();
        BuildRuntimeStates();
    }

    void Update()
    {
        if (player == null)
        {
            player = PlayerController.Instance;
        }

        if (player == null || !player.gameObject.activeSelf || waves.Count == 0)
        {
            return;
        }

        Wave currentWave = waves[waveNumber];
        SpawnRuntimeState currentState = runtimeStates[waveNumber];
        currentState.spawnTimer += Time.deltaTime;

        if (currentState.spawnTimer < currentState.currentSpawnInterval)
        {
            return;
        }

        currentState.spawnTimer = 0f;
        SpawnPack(currentWave);
        currentState.completedSpawnCycles++;

        if (currentState.completedSpawnCycles < currentWave.enemiesPerWave)
        {
            return;
        }

        AdvanceToNextWave(currentWave, currentState);
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
        int packSize = Mathf.Max(1, Mathf.RoundToInt(wave.packSize * modifiers.quantity));

        for (int i = 0; i < packSize; i++)
        {
            SpawnEnemy(wave);
        }
    }

    private void SpawnEnemy(Wave wave)
    {
        GameObject enemyObject = Instantiate(wave.enemyPrefab, RandomSpawnPoint(), transform.rotation);
        Enemy enemy = enemyObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.Initialize(BuildSpawnContext());
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
        float damageMultiplier = 1f + GetMapModifier(MapStatType.EnemyDamage);
        float healthMultiplier = 1f + GetMapModifier(MapStatType.EnemyHealth);
        float moveSpeedMultiplier = 1f + GetMapModifier(MapStatType.EnemyMoveSpeed);
        float experienceMultiplier = 1f + GetMapModifier(MapStatType.ExperienceWorth);
        float dropChanceMultiplier = 1f + GetMapModifier(MapStatType.DropChance);

        switch (rarity)
        {
            case EnemyRarity.Uncommon:
                return new EnemySpawnContext
                {
                    rarity = rarity,
                    healthMultiplier = 1.5f * healthMultiplier,
                    damageMultiplier = 1.2f * damageMultiplier,
                    moveSpeedMultiplier = 1.05f * moveSpeedMultiplier,
                    experienceMultiplier = 1.5f * experienceMultiplier,
                    dropChanceMultiplier = dropChanceMultiplier,
                };

            case EnemyRarity.Rare:
                return new EnemySpawnContext
                {
                    rarity = rarity,
                    healthMultiplier = 2.5f * healthMultiplier,
                    damageMultiplier = 1.5f * damageMultiplier,
                    moveSpeedMultiplier = 1.1f * moveSpeedMultiplier,
                    experienceMultiplier = 2.5f * experienceMultiplier,
                    dropChanceMultiplier = dropChanceMultiplier,
                };

            default:
                return new EnemySpawnContext
                {
                    rarity = EnemyRarity.Normal,
                    healthMultiplier = healthMultiplier,
                    damageMultiplier = damageMultiplier,
                    moveSpeedMultiplier = moveSpeedMultiplier,
                    experienceMultiplier = experienceMultiplier,
                    dropChanceMultiplier = dropChanceMultiplier,
                };
        }
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

    private void ApplyRunModifiers()
    {
        modifiers.quantity = 1f + GetMapModifier(MapStatType.EnemyQuantity);
        modifiers.quality = 1f + GetMapModifier(MapStatType.EnemyQuality);
    }

    private float GetMapModifier(MapStatType statType)
    {
        if (RunData.SelectedMap == null)
        {
            return 0f;
        }

        return RunData.SelectedMap.GetModifier(statType) / 100f;
    }

    private Vector2 RandomSpawnPoint()
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
}
