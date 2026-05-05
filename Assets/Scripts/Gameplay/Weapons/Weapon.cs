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
        stats.baseCriticalChance = baseStats.criticalChance;
        stats.baseAttackSpeed = baseStats.attackSpeed;
        stats.baseKnockback = baseStats.knockback;
        stats.baseRange = baseStats.range;
        stats.duration = baseStats.duration;
        stats.cooldown = baseStats.cooldown;
        stats.projectileSpeed = baseStats.projectileSpeed;
        stats.bounceCount = baseStats.bounceCount;

        stats.damageMultiplier = 0f;
        stats.criticalChanceMultiplier = 0f;
        stats.criticalDamageBonus = 0f;
        stats.attackSpeedMultiplier = 0f;
        stats.knockbackMultiplier = 0f;
        stats.rangeMultiplier = 0f;
        stats.globalDamageMultiplier = 0f;
        stats.globalCriticalChanceMultiplier = 0f;
        stats.globalCriticalDamageBonus = 0f;
        stats.globalAttackSpeedMultiplier = 0f;
        stats.globalKnockbackMultiplier = 0f;
        stats.globalRangeMultiplier = 0f;
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

                case StatType.CriticalChance:
                    stats.criticalChanceMultiplier += stat.Value;
                    break;

                case StatType.CriticalDamage:
                    stats.criticalDamageBonus += stat.Value;
                    break;

                case StatType.Knockback:
                    stats.knockbackMultiplier += stat.Value;
                    break;

                case StatType.Range:
                    stats.rangeMultiplier += stat.Value;
                    break;

                case StatType.Duration:
                    float maxDuration = data != null && data.weaponName == "Area Weapon" ? 5f : float.MaxValue;
                    stats.duration = Mathf.Min(stats.duration + stat.Value, maxDuration);
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

    public void ApplyGlobalPlayerStat(PlayerStatType statType, float value)
    {
        switch (statType)
        {
            case PlayerStatType.Damage:
                stats.globalDamageMultiplier += value;
                break;

            case PlayerStatType.CriticalChance:
                stats.globalCriticalChanceMultiplier += value;
                break;

            case PlayerStatType.CriticalDamage:
                stats.globalCriticalDamageBonus += value;
                break;

            case PlayerStatType.AttackSpeed:
                stats.globalAttackSpeedMultiplier += value;
                break;

            case PlayerStatType.Range:
                stats.globalRangeMultiplier += value;
                break;

            case PlayerStatType.Knockback:
                stats.globalKnockbackMultiplier += value;
                break;
        }
    }
}
