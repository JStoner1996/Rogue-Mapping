using UnityEngine;
using UnityEngine.Serialization;

// Defines the gameplay identity of an enemy archetype and the scaling hooks it provides.
[CreateAssetMenu(fileName = "EnemyArchetypeDefinition", menuName = "Enemies/Archetype Definition")]
public class EnemyArchetypeDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private EnemyArchetype archetype = EnemyArchetype.Fodder;
    [SerializeField] private string displayName;

    [Header("Spawn Model")]
    [SerializeField] private EnemySpawnRole spawnRole = EnemySpawnRole.Ambient;
    [FormerlySerializedAs("spawnWeight")]
    [SerializeField, Min(0f)] private float ambientSpawnWeight = 1f;

    [Header("Progression")]
    [SerializeField] private int itemLevelOffset;

    [Header("Stat Multipliers")]
    [SerializeField, Min(0f)] private float healthMultiplier = 1f;
    [SerializeField, Min(0f)] private float damageMultiplier = 1f;
    [SerializeField, Min(0f)] private float moveSpeedMultiplier = 1f;
    [SerializeField, Min(0f)] private float experienceMultiplier = 1f;
    [SerializeField, Min(0f)] private float dropChanceMultiplier = 1f;

    public EnemyArchetype Archetype => archetype;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? archetype.ToString() : displayName;
    public EnemySpawnRole SpawnRole => spawnRole;
    public bool UsesAmbientSpawnWeight => spawnRole == EnemySpawnRole.Ambient;
    public int ItemLevelOffset => itemLevelOffset;
    public float AmbientSpawnWeight => UsesAmbientSpawnWeight ? ambientSpawnWeight : 0f;
    public float HealthMultiplier => healthMultiplier;
    public float DamageMultiplier => damageMultiplier;
    public float MoveSpeedMultiplier => moveSpeedMultiplier;
    public float ExperienceMultiplier => experienceMultiplier;
    public float DropChanceMultiplier => dropChanceMultiplier;
}
