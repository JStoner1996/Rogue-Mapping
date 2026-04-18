using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradePreset", menuName = "Weapons/Upgrade Preset")]
public class UpgradePreset : ScriptableObject
{
    public List<StatRoll> rolls = new List<StatRoll>();

    private void OnEnable()
    {
        // Prevent overwriting if already set in inspector
        if (rolls != null && rolls.Count > 0)
            return;

        rolls = new List<StatRoll>()
        {
            // =====================
            // DAMAGE
            // =====================
            new StatRoll { statType = StatType.Damage, minValue = 0.05f, maxValue = 0.15f, weight = 1 },
            new StatRoll { statType = StatType.Damage, minValue = .16f, maxValue = .25f, weight = 2 },
            new StatRoll { statType = StatType.Damage, minValue = .26f, maxValue = .4f, weight = 3 },
            new StatRoll { statType = StatType.Damage, minValue = 0.40f, maxValue = 0.50f, weight = 6 },

            // =====================
            // ATTACK SPEED
            // =====================
            new StatRoll { statType = StatType.AttackSpeed, minValue = -0.05f, maxValue = -0.075f, weight = 1 },
            new StatRoll { statType = StatType.AttackSpeed, minValue = -0.075f, maxValue = -0.1f, weight = 2 },
            new StatRoll { statType = StatType.AttackSpeed, minValue = -0.1f, maxValue = -0.15f, weight = 3 },
            new StatRoll { statType = StatType.AttackSpeed, minValue = -0.2f, maxValue = -0.25f, weight = 6 },
            new StatRoll { statType = StatType.AttackSpeed, minValue = -0.25f, maxValue = -0.35f, weight = 7},
    

            // =====================
            // RANGE
            // =====================
            new StatRoll { statType = StatType.Range, minValue = 0.05f, maxValue = 0.1f, weight = 1 },
            new StatRoll { statType = StatType.Range, minValue = 0.1f, maxValue = 0.15f, weight = 2 },
            new StatRoll { statType = StatType.Range, minValue = 0.15f, maxValue = 0.25f, weight = 3 },


            // =====================
            // KNOCKBACK (% increase)
            // =====================
            new StatRoll { statType = StatType.Knockback, minValue = 0.10f, maxValue = 0.15f, weight = 1 },
            new StatRoll { statType = StatType.Knockback, minValue = 0.15f, maxValue = 0.25f, weight = 2 },
            new StatRoll { statType = StatType.Knockback, minValue = 0.25f, maxValue = 0.35f, weight = 3 },
            new StatRoll { statType = StatType.Knockback, minValue = 0.35f, maxValue = 0.45f, weight = 4 },
            new StatRoll { statType = StatType.Knockback, minValue = 0.45f, maxValue = 0.50f, weight = 5 },

            // =====================
            // DURATION
            // =====================
            new StatRoll { statType = StatType.Duration, minValue = 0.5f, maxValue = 1f, weight = 4 },
            new StatRoll { statType = StatType.Duration, minValue = 1f, maxValue = 2f, weight = 5 },
            new StatRoll { statType = StatType.Duration, minValue = 2f, maxValue = 3f, weight = 6 },

            // =====================
            // COOLDOWN (reduction-style rolls)
            // =====================
            new StatRoll { statType = StatType.Cooldown, minValue = -0.2f, maxValue = -0.1f, weight = 4 },
            new StatRoll { statType = StatType.Cooldown, minValue = -0.5f, maxValue = -0.3f, weight = 5 },
            new StatRoll { statType = StatType.Cooldown, minValue = -0.6f, maxValue = -0.3f, weight = 6 },

            // =====================
            // BOUNCE COUNT (flat increase)
            // =====================
            new StatRoll { statType = StatType.BounceCount, minValue = 1f, maxValue = 1f, weight = 1 },
            new StatRoll { statType = StatType.BounceCount, minValue = 2, maxValue = 3f, weight = 4 },
            new StatRoll { statType = StatType.BounceCount, minValue = 4, maxValue = 5f, weight = 6 },

        };
    }
}