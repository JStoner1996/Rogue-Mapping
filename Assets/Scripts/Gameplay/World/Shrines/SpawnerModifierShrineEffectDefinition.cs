using UnityEngine;

// Applies a run-scoped or temporary spawn modifier when a shrine completes.
[CreateAssetMenu(fileName = "SpawnerModifierShrineEffect", menuName = "Shrines/Effects/Spawner Modifier")]
public class SpawnerModifierShrineEffectDefinition : ShrineEffectDefinition
{
    [SerializeField] private EnemySpawnerModifierType modifierType = EnemySpawnerModifierType.EnemyQuality;
    [SerializeField] private float modifierPercent = 20f;
    [SerializeField, Min(0f)] private float durationSeconds = 0f;

    public override void Activate(ShrineObjective shrine)
    {
        if (shrine != null)
        {
            shrine.ApplySpawnerModifier(
                modifierType,
                shrine.ScaleEffectValue(modifierPercent / 100f),
                shrine.ScaleDuration(durationSeconds));
        }
    }
}
