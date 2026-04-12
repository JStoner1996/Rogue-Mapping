using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private List<PlayerStatRoll> upgradeRolls = new List<PlayerStatRoll>();

    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private PlayerCollector playerCollector;
    private WeaponController weaponController;

    private readonly Dictionary<PlayerStatType, float> totals = new Dictionary<PlayerStatType, float>();

    public IReadOnlyList<PlayerStatRoll> UpgradeRolls => upgradeRolls;

    public void Configure(
        PlayerMovement movementComponent,
        PlayerHealth healthComponent,
        PlayerCollector collectorComponent,
        WeaponController configuredWeaponController)
    {
        playerMovement = movementComponent;
        playerHealth = healthComponent;
        playerCollector = collectorComponent;
        weaponController = configuredWeaponController;
    }

    void OnEnable()
    {
        if (upgradeRolls != null && upgradeRolls.Count > 0)
        {
            return;
        }

        upgradeRolls = new List<PlayerStatRoll>
        {
            new PlayerStatRoll { statType = PlayerStatType.MaximumHealth, minValue = 0.08f, maxValue = 0.14f, weight = 1 },
            new PlayerStatRoll { statType = PlayerStatType.MaximumHealth, minValue = 0.14f, maxValue = 0.22f, weight = 3 },
            new PlayerStatRoll { statType = PlayerStatType.MaximumHealth, minValue = 0.22f, maxValue = 0.30f, weight = 6 },

            new PlayerStatRoll { statType = PlayerStatType.MovementSpeed, minValue = 0.05f, maxValue = 0.08f, weight = 1 },
            new PlayerStatRoll { statType = PlayerStatType.MovementSpeed, minValue = 0.08f, maxValue = 0.13f, weight = 3 },
            new PlayerStatRoll { statType = PlayerStatType.MovementSpeed, minValue = 0.13f, maxValue = 0.18f, weight = 6 },

            new PlayerStatRoll { statType = PlayerStatType.PickupRange, minValue = 0.08f, maxValue = 0.13f, weight = 1 },
            new PlayerStatRoll { statType = PlayerStatType.PickupRange, minValue = 0.13f, maxValue = 0.20f, weight = 3 },
            new PlayerStatRoll { statType = PlayerStatType.PickupRange, minValue = 0.20f, maxValue = 0.28f, weight = 6 },

            new PlayerStatRoll { statType = PlayerStatType.Damage, minValue = 0.05f, maxValue = 0.09f, weight = 1 },
            new PlayerStatRoll { statType = PlayerStatType.Damage, minValue = 0.09f, maxValue = 0.14f, weight = 3 },
            new PlayerStatRoll { statType = PlayerStatType.Damage, minValue = 0.14f, maxValue = 0.20f, weight = 6 },

            new PlayerStatRoll { statType = PlayerStatType.AttackSpeed, minValue = 0.05f, maxValue = 0.08f, weight = 1 },
            new PlayerStatRoll { statType = PlayerStatType.AttackSpeed, minValue = 0.08f, maxValue = 0.13f, weight = 3 },
            new PlayerStatRoll { statType = PlayerStatType.AttackSpeed, minValue = 0.13f, maxValue = 0.18f, weight = 6 },

            new PlayerStatRoll { statType = PlayerStatType.Range, minValue = 0.06f, maxValue = 0.10f, weight = 1 },
            new PlayerStatRoll { statType = PlayerStatType.Range, minValue = 0.10f, maxValue = 0.15f, weight = 3 },
            new PlayerStatRoll { statType = PlayerStatType.Range, minValue = 0.15f, maxValue = 0.22f, weight = 6 },

            new PlayerStatRoll { statType = PlayerStatType.Knockback, minValue = 0.07f, maxValue = 0.12f, weight = 1 },
            new PlayerStatRoll { statType = PlayerStatType.Knockback, minValue = 0.12f, maxValue = 0.18f, weight = 3 },
            new PlayerStatRoll { statType = PlayerStatType.Knockback, minValue = 0.18f, maxValue = 0.26f, weight = 6 },
        };
    }

    public void ApplyUpgrade(PlayerStatUpgradeResult upgrade)
    {
        foreach (var stat in upgrade.stats)
        {
            ApplyStat(stat.Key, stat.Value);
        }
    }

    public void ApplyToWeapon(Weapon weapon)
    {
        if (weapon == null)
        {
            return;
        }

        foreach (var stat in totals)
        {
            weapon.ApplyGlobalPlayerStat(stat.Key, stat.Value);
        }
    }

    private void ApplyStat(PlayerStatType statType, float value)
    {
        if (!totals.ContainsKey(statType))
        {
            totals[statType] = 0f;
        }

        totals[statType] += value;

        switch (statType)
        {
            case PlayerStatType.MaximumHealth:
                playerHealth?.ApplyMaxHealthModifier(value);
                break;

            case PlayerStatType.MovementSpeed:
                playerMovement?.ApplyMoveSpeedModifier(value);
                break;

            case PlayerStatType.PickupRange:
                playerCollector?.ApplyPickupRangeModifier(value);
                break;

            case PlayerStatType.Damage:
            case PlayerStatType.AttackSpeed:
            case PlayerStatType.Range:
            case PlayerStatType.Knockback:
                weaponController?.ApplyGlobalPlayerStat(statType, value);
                break;
        }
    }
}
