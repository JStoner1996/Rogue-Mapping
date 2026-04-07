using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponStatsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text weaponName;
    [SerializeField] private TMP_Text weaponStats;

    public void Initialize(Weapon weapon)
    {
        weaponName.text = weapon.data.weaponName;
        weaponStats.text = BuildStatsText(weapon);
    }

    private string BuildStatsText(Weapon weapon)
    {
        var stats = weapon.stats;
        var allowed = weapon.data.allowedStats;

        string desc = "";

        if (allowed.Contains(StatType.Damage))
            desc += $"Damage: {stats.Damage:F2}\n";

        if (allowed.Contains(StatType.AttackSpeed))
            desc += $"Attack Speed: {stats.AttackSpeed:F2}\n";

        if (allowed.Contains(StatType.Knockback))
            desc += $"Knockback: {stats.Knockback:F2}\n";

        if (allowed.Contains(StatType.Range))
            desc += $"Range: {stats.Range:F2}\n";

        if (allowed.Contains(StatType.Duration))
            desc += $"Duration: {stats.duration:F2}\n";

        if (allowed.Contains(StatType.Cooldown))
            desc += $"Cooldown: {stats.cooldown:F2}\n";

        if (allowed.Contains(StatType.BounceCount))
            desc += $"Bounce Count: {stats.bounceCount}";


        return desc.TrimEnd('\n');
    }
}