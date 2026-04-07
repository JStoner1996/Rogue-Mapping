using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData data;

    public RuntimeStats stats = new RuntimeStats();

    public void InitializeStats()
    {
        var baseStats = data.baseStats;

        stats.baseDamage = baseStats.damage;
        stats.baseAttackSpeed = baseStats.attackSpeed;
        stats.baseRange = baseStats.range;
        stats.duration = baseStats.duration;
        stats.cooldown = baseStats.cooldown;
        stats.bounceCount = baseStats.bounceCount;
    }

    public virtual void ManualUpdate(float deltaTime)
    {
        // Base weapon does nothing by default
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