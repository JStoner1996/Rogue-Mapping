public struct EnemySpawnContext
{
    public EnemyRarity rarity;
    public float healthMultiplier;
    public float damageMultiplier;
    public float moveSpeedMultiplier;
    public float experienceMultiplier;

    public static EnemySpawnContext Default => new EnemySpawnContext
    {
        rarity = EnemyRarity.Normal,
        healthMultiplier = 1f,
        damageMultiplier = 1f,
        moveSpeedMultiplier = 1f,
        experienceMultiplier = 1f,
    };
}
