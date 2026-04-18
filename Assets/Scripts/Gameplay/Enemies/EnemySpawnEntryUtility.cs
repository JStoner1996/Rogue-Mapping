using System.Collections.Generic;
using UnityEngine;

// Centralizes spawn-pool prefab inspection so EnemySpawner can stay focused on orchestration.
public static class EnemySpawnEntryUtility
{
    public static bool HasWeightedEntries(
        IReadOnlyList<EnemySpawner.PackEntry> entries,
        System.Func<EnemySpawner.PackEntry, float> getWeight)
    {
        if (entries == null || getWeight == null)
        {
            return false;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (getWeight(entries[i]) > 0f)
            {
                return true;
            }
        }

        return false;
    }

    public static float GetAmbientSpawnWeight(EnemySpawner.PackEntry entry)
    {
        return TryGetPackDefinition(entry, out _, out EnemyArchetypeDefinition archetypeDefinition)
            ? archetypeDefinition != null ? archetypeDefinition.AmbientSpawnWeight : 0f
            : 0f;
    }

    public static float GetEventSpawnWeight(EnemySpawner.PackEntry entry, EnemyArchetype archetype)
    {
        if (!TryGetPackDefinition(entry, out _, out EnemyArchetypeDefinition archetypeDefinition))
        {
            return 0f;
        }

        if (archetypeDefinition == null
            || archetypeDefinition.SpawnRole != EnemySpawnRole.EventOnly
            || archetypeDefinition.Archetype != archetype)
        {
            return 0f;
        }

        return 1f;
    }

    public static string GetMissingEventSpawnReason(IReadOnlyList<EnemySpawner.PackEntry> entries, EnemyArchetype archetype)
    {
        if (entries == null || entries.Count == 0)
        {
            return $"Event spawn for '{archetype}' could not start because the Event Spawn Pool is empty.";
        }

        for (int i = 0; i < entries.Count; i++)
        {
            string invalidReason = GetInvalidEventSpawnReason(entries[i], archetype);
            if (!string.IsNullOrEmpty(invalidReason))
            {
                return invalidReason;
            }
        }

        return $"Event spawn for '{archetype}' could not start because no valid {archetype} + EventOnly entry was found in the Event Spawn Pool.";
    }

    public static string GetInvalidEventSpawnReason(EnemySpawner.PackEntry entry, EnemyArchetype archetype)
    {
        if (entry == null || entry.enemyPrefab == null)
        {
            return null;
        }

        if (!TryGetPackDefinition(entry, out _, out EnemyArchetypeDefinition archetypeDefinition))
        {
            return $"Event spawn for '{archetype}' could not start because event pack '{entry.enemyPrefab.name}' does not have an Enemy component.";
        }

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

        return null;
    }

    public static EnemySpawner.PackEntry RollWeightedEntry(
        IReadOnlyList<EnemySpawner.PackEntry> entries,
        System.Func<EnemySpawner.PackEntry, float> getWeight)
    {
        if (entries == null || getWeight == null)
        {
            return null;
        }

        float totalWeight = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            totalWeight += getWeight(entries[i]);
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float roll = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            EnemySpawner.PackEntry entry = entries[i];
            currentWeight += getWeight(entry);

            if (roll <= currentWeight)
            {
                return entry;
            }
        }

        return null;
    }

    public static bool TryGetPackDefinition(
        EnemySpawner.PackEntry entry,
        out Enemy enemyTemplate,
        out EnemyArchetypeDefinition archetypeDefinition)
    {
        enemyTemplate = null;
        archetypeDefinition = null;

        if (entry == null || entry.enemyPrefab == null)
        {
            return false;
        }

        enemyTemplate = entry.enemyPrefab.GetComponent<Enemy>();
        archetypeDefinition = enemyTemplate != null ? enemyTemplate.ArchetypeDefinition : null;
        return enemyTemplate != null;
    }

    public static void CollectPoolPrefabs(IReadOnlyList<EnemySpawner.PackEntry> entries, ISet<GameObject> output)
    {
        if (entries == null || output == null)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            EnemySpawner.PackEntry entry = entries[i];

            if (entry != null && entry.enemyPrefab != null)
            {
                output.Add(entry.enemyPrefab);
            }
        }
    }
}
