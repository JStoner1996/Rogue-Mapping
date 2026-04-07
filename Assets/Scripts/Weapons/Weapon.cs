using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Runtime Data")]
    [SerializeField] private WeaponData data;

    public WeaponData Data => data;

    [System.NonSerialized] public RuntimeStats stats = new RuntimeStats();

    protected bool isInitialized;

    public virtual void Initialize(WeaponData weaponData)
    {
        data = weaponData;
        InitializeStats();
        isInitialized = true;
    }

    private void InitializeStats()
    {
        if (data == null)
        {
            Debug.LogError($"{name}: WeaponData is missing!");
            return;
        }

        var baseStats = data.baseStats;

        stats.baseDamage = baseStats.damage;
        stats.baseAttackSpeed = baseStats.attackSpeed;
        stats.baseKnockback = baseStats.knockback;
        stats.baseRange = baseStats.range;
        stats.duration = baseStats.duration;
        stats.cooldown = baseStats.cooldown;
        stats.bounceCount = baseStats.bounceCount;

        stats.damageMultiplier = 0f;
        stats.attackSpeedMultiplier = 0f;
        stats.knockbackMultiplier = 0f;
        stats.rangeMultiplier = 0f;
    }

    public virtual void ManualUpdate(float deltaTime)
    {
        if (!isInitialized)
            return;
    }

    public void ApplyUpgrade(WeaponUpgradeResult upgrade)
    {
        foreach (var stat in upgrade.stats)
        {
            switch (stat.Key)
            {
                case StatType.AttackSpeed:
                    stats.attackSpeedMultiplier += stat.Value;
                    break;

                case StatType.Damage:
                    stats.damageMultiplier += stat.Value;
                    break;

                case StatType.Knockback:
                    stats.knockbackMultiplier += stat.Value;
                    break;

                case StatType.Range:
                    stats.rangeMultiplier += stat.Value;
                    break;

                case StatType.Duration:
                    stats.duration += stat.Value;
                    break;

                case StatType.Cooldown:
                    stats.cooldown += stat.Value;
                    break;

                case StatType.BounceCount:
                    stats.bounceCount += (int)stat.Value;
                    break;
            }
        }
    }
}