using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStats : MonoBehaviour
{
    private static readonly PlayerStatRoll[] DefaultUpgradeRolls =
    {
        new() { statType = PlayerStatType.MaximumHealth, minValue = 0.08f, maxValue = 0.14f, weight = 1 },
        new() { statType = PlayerStatType.MaximumHealth, minValue = 0.14f, maxValue = 0.22f, weight = 3 },
        new() { statType = PlayerStatType.MaximumHealth, minValue = 0.22f, maxValue = 0.30f, weight = 6 },
        new() { statType = PlayerStatType.Armor, usesFlatValue = true, minValue = 6f, maxValue = 12f, weight = 1 },
        new() { statType = PlayerStatType.Armor, usesFlatValue = true, minValue = 13f, maxValue = 20f, weight = 3 },
        new() { statType = PlayerStatType.Armor, usesFlatValue = true, minValue = 21f, maxValue = 30f, weight = 6 },
        new() { statType = PlayerStatType.Armor, minValue = 0.06f, maxValue = 0.10f, weight = 1 },
        new() { statType = PlayerStatType.Armor, minValue = 0.10f, maxValue = 0.15f, weight = 3 },
        new() { statType = PlayerStatType.Armor, minValue = 0.15f, maxValue = 0.22f, weight = 6 },
        new() { statType = PlayerStatType.Evasion, usesFlatValue = true, minValue = 6f, maxValue = 12f, weight = 1 },
        new() { statType = PlayerStatType.Evasion, usesFlatValue = true, minValue = 13f, maxValue = 20f, weight = 3 },
        new() { statType = PlayerStatType.Evasion, usesFlatValue = true, minValue = 21f, maxValue = 30f, weight = 6 },
        new() { statType = PlayerStatType.Evasion, minValue = 0.06f, maxValue = 0.10f, weight = 1 },
        new() { statType = PlayerStatType.Evasion, minValue = 0.10f, maxValue = 0.15f, weight = 3 },
        new() { statType = PlayerStatType.Evasion, minValue = 0.15f, maxValue = 0.22f, weight = 6 },
        new() { statType = PlayerStatType.Barrier, usesFlatValue = true, minValue = 10f, maxValue = 20f, weight = 1 },
        new() { statType = PlayerStatType.Barrier, usesFlatValue = true, minValue = 21f, maxValue = 35f, weight = 3 },
        new() { statType = PlayerStatType.Barrier, usesFlatValue = true, minValue = 36f, maxValue = 50f, weight = 6 },
        new() { statType = PlayerStatType.Barrier, minValue = 0.10f, maxValue = 0.15f, weight = 1 },
        new() { statType = PlayerStatType.Barrier, minValue = 0.15f, maxValue = 0.22f, weight = 3 },
        new() { statType = PlayerStatType.Barrier, minValue = 0.22f, maxValue = 0.30f, weight = 6 },
        new() { statType = PlayerStatType.MovementSpeed, minValue = 0.05f, maxValue = 0.08f, weight = 1 },
        new() { statType = PlayerStatType.MovementSpeed, minValue = 0.08f, maxValue = 0.13f, weight = 3 },
        new() { statType = PlayerStatType.MovementSpeed, minValue = 0.13f, maxValue = 0.18f, weight = 6 },
        new() { statType = PlayerStatType.PickupRange, minValue = 0.08f, maxValue = 0.13f, weight = 1 },
        new() { statType = PlayerStatType.PickupRange, minValue = 0.13f, maxValue = 0.20f, weight = 3 },
        new() { statType = PlayerStatType.PickupRange, minValue = 0.20f, maxValue = 0.28f, weight = 6 },
        new() { statType = PlayerStatType.Damage, minValue = 0.05f, maxValue = 0.09f, weight = 1 },
        new() { statType = PlayerStatType.Damage, minValue = 0.09f, maxValue = 0.14f, weight = 3 },
        new() { statType = PlayerStatType.Damage, minValue = 0.14f, maxValue = 0.20f, weight = 6 },
        new() { statType = PlayerStatType.AttackSpeed, minValue = 0.05f, maxValue = 0.08f, weight = 1 },
        new() { statType = PlayerStatType.AttackSpeed, minValue = 0.08f, maxValue = 0.13f, weight = 3 },
        new() { statType = PlayerStatType.AttackSpeed, minValue = 0.13f, maxValue = 0.18f, weight = 6 },
        new() { statType = PlayerStatType.Range, minValue = 0.06f, maxValue = 0.10f, weight = 1 },
        new() { statType = PlayerStatType.Range, minValue = 0.10f, maxValue = 0.15f, weight = 3 },
        new() { statType = PlayerStatType.Range, minValue = 0.15f, maxValue = 0.22f, weight = 6 },
        new() { statType = PlayerStatType.Knockback, minValue = 0.07f, maxValue = 0.12f, weight = 1 },
        new() { statType = PlayerStatType.Knockback, minValue = 0.12f, maxValue = 0.18f, weight = 3 },
        new() { statType = PlayerStatType.Knockback, minValue = 0.18f, maxValue = 0.26f, weight = 6 },
    };

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

        upgradeRolls = new List<PlayerStatRoll>(DefaultUpgradeRolls);
    }

    public void ApplyUpgrade(PlayerStatUpgradeResult upgrade)
    {
        foreach (PlayerStatUpgradeResult.PlayerStatUpgradeEntry stat in upgrade.GetEntries())
        {
            ApplyStat(stat.statType, stat.value, stat.usesFlatValue);
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

    public void ApplyEquipmentSummary(EquipmentStatSummary summary)
    {
        if (summary == null)
        {
            return;
        }

        ApplyEquipmentEntry(summary.GetEntry(EquipmentStatType.MaximumHealth), PlayerStatType.MaximumHealth, supportsFlatValue: true);
        ApplyEquipmentEntry(summary.GetEntry(EquipmentStatType.Armor), PlayerStatType.Armor, supportsFlatValue: true);
        ApplyEquipmentEntry(summary.GetEntry(EquipmentStatType.Barrier), PlayerStatType.Barrier, supportsFlatValue: true);
        ApplyEquipmentEntry(summary.GetEntry(EquipmentStatType.Evasion), PlayerStatType.Evasion, supportsFlatValue: true);
        ApplyEquipmentEntry(summary.GetEntry(EquipmentStatType.HealthRegen), PlayerStatType.HealthRegen);
        ApplyEquipmentEntry(summary.GetEntry(EquipmentStatType.MovementSpeed), PlayerStatType.MovementSpeed);
        ApplyEquipmentEntry(summary.GetEntry(EquipmentStatType.PickupRange), PlayerStatType.PickupRange);
        ApplyEquipmentEntry(summary.GetEntry(EquipmentStatType.Damage), PlayerStatType.Damage);
        ApplyEquipmentEntry(summary.GetEntry(EquipmentStatType.AttackSpeed), PlayerStatType.AttackSpeed);
        ApplyEquipmentEntry(summary.GetEntry(EquipmentStatType.Range), PlayerStatType.Range);
        ApplyEquipmentEntry(summary.GetEntry(EquipmentStatType.Knockback), PlayerStatType.Knockback);
    }

    public float GetTotal(PlayerStatType statType)
    {
        return totals.TryGetValue(statType, out float value) ? value : 0f;
    }

    private void ApplyStat(PlayerStatType statType, float value, bool usesFlatValue = false)
    {
        if (!usesFlatValue)
        {
            totals[statType] = GetTotal(statType) + value;
        }

        switch (statType)
        {
            case PlayerStatType.MaximumHealth:
                if (usesFlatValue) playerHealth?.ApplyFlatMaxHealthModifier(value);
                else playerHealth?.ApplyMaxHealthModifier(value);
                break;

            case PlayerStatType.Armor:
                if (usesFlatValue) playerHealth?.ApplyFlatArmorModifier(value);
                else playerHealth?.ApplyArmorModifier(value);
                break;

            case PlayerStatType.Barrier:
                if (usesFlatValue) playerHealth?.ApplyFlatBarrierModifier(value);
                else playerHealth?.ApplyBarrierModifier(value);
                break;

            case PlayerStatType.Evasion:
                if (usesFlatValue) playerHealth?.ApplyFlatEvasionModifier(value);
                else playerHealth?.ApplyEvasionModifier(value);
                break;

            case PlayerStatType.HealthRegen:
                playerHealth?.ApplyHealthRegenModifier(value);
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

    private void ApplyEquipmentEntry(EquipmentStatSummaryEntry entry, PlayerStatType targetStatType, bool supportsFlatValue = false)
    {
        if (entry == null)
        {
            return;
        }

        if (supportsFlatValue && !Mathf.Approximately(entry.flatValue, 0f))
        {
            ApplyStat(targetStatType, entry.flatValue, usesFlatValue: true);
        }

        if (!Mathf.Approximately(entry.percentValue, 0f))
        {
            ApplyStat(targetStatType, entry.percentValue);
        }

        if (!supportsFlatValue && !Mathf.Approximately(entry.flatValue, 0f))
        {
            ApplyStat(targetStatType, entry.flatValue);
        }
    }
}
