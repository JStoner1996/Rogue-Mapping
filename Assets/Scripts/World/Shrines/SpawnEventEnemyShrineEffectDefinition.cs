using UnityEngine;

// Spawns event-only enemies like minibosses when a shrine completes.
[CreateAssetMenu(fileName = "SpawnEventEnemyShrineEffect", menuName = "Shrines/Effects/Spawn Event Enemy")]
public class SpawnEventEnemyShrineEffectDefinition : ShrineEffectDefinition
{
    [SerializeField] private EnemyArchetype enemyArchetype = EnemyArchetype.Miniboss;
    [SerializeField, Min(1)] private int spawnCount = 1;

    public override void Activate(ShrineObjective shrine)
    {
        if (shrine != null)
        {
            shrine.TrySpawnEventEnemies(enemyArchetype, spawnCount);
        }
    }
}
