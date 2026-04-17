using System.Collections.Generic;
using UnityEngine;

// Centralized enemy pooling, similar in spirit to the pickup pool service.
public class EnemyPools : MonoBehaviour
{
    public static EnemyPools Instance { get; private set; }

    private readonly Dictionary<GameObject, ObjectPool<Enemy>> enemyPools = new Dictionary<GameObject, ObjectPool<Enemy>>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void EnsurePools(IReadOnlyList<GameObject> prefabs, int initialPoolSizePerEnemy)
    {
        if (prefabs == null)
        {
            return;
        }

        for (int i = 0; i < prefabs.Count; i++)
        {
            RegisterPool(prefabs[i], initialPoolSizePerEnemy);
        }
    }

    public Enemy GetEnemy(GameObject prefab, int initialPoolSizePerEnemy)
    {
        if (prefab == null)
        {
            return null;
        }

        RegisterPool(prefab, initialPoolSizePerEnemy);
        return enemyPools.TryGetValue(prefab, out ObjectPool<Enemy> pool) ? pool.Get() : null;
    }

    public void ReturnEnemy(Enemy enemy, GameObject prefab)
    {
        if (enemy == null || prefab == null)
        {
            return;
        }

        if (!enemyPools.TryGetValue(prefab, out ObjectPool<Enemy> pool))
        {
            Destroy(enemy.gameObject);
            return;
        }

        pool.ReturnToPool(enemy);
    }

    private void RegisterPool(GameObject prefab, int initialPoolSizePerEnemy)
    {
        if (prefab == null || enemyPools.ContainsKey(prefab))
        {
            return;
        }

        Enemy enemyPrefab = prefab.GetComponent<Enemy>();
        if (enemyPrefab == null)
        {
            return;
        }

        enemyPools.Add(prefab, new ObjectPool<Enemy>(enemyPrefab, initialPoolSizePerEnemy));
    }
}
