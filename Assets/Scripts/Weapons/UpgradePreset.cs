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
            new StatRoll { statType = StatType.Damage, minValue = 1f, maxValue = 2f, weight = 1 },
            new StatRoll { statType = StatType.Damage, minValue = 2.01f, maxValue = 4f, weight = 2 },
            new StatRoll { statType = StatType.Damage, minValue = 4.01f, maxValue = 6f, weight = 3 },

            // =====================
            // ATTACK SPEED
            // =====================
            new StatRoll { statType = StatType.AttackSpeed, minValue = -0.05f, maxValue = -0.075f, weight = 1 },
            new StatRoll { statType = StatType.AttackSpeed, minValue = -0.075f, maxValue = -0.1f, weight = 1 },
            new StatRoll { statType = StatType.AttackSpeed, minValue = -0.1f, maxValue = -0.15f, weight = 1 },
    

            // =====================
            // RANGE
            // =====================
            new StatRoll { statType = StatType.Range, minValue = 0.05f, maxValue = 0.1f, weight = 1 },
            new StatRoll { statType = StatType.Range, minValue = 0.1f, maxValue = .15f, weight = 2 },
            new StatRoll { statType = StatType.Range, minValue = .15f, maxValue = .25f, weight = 3 },

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
            new StatRoll { statType = StatType.Cooldown, minValue = -0.4f, maxValue = -0.2f, weight = 5 },
            new StatRoll { statType = StatType.Cooldown, minValue = -0.6f, maxValue = -0.3f, weight = 6 },
        };
    }
}