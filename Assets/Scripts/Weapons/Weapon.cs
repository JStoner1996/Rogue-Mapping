using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData data;

    public RuntimeStats stats = new RuntimeStats();

    public void InitializeStats()
    {
        var baseStats = data.baseStats;

        stats.damage = baseStats.damage;
        stats.baseAttackSpeed = baseStats.attackSpeed;
        stats.baseRange = baseStats.range;
        stats.duration = baseStats.duration;
        stats.cooldown = baseStats.cooldown;
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
                    stats.damage += stat.Value;
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
            }
        }
    }
}