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

        string desc = "";

        desc += $"Damage: {stats.damage}\n";
        desc += $"Attack Speed: {stats.AttackSpeed:F2}\n";
        desc += $"Range: {stats.Range:F2}\n";
        desc += $"Duration: {stats.duration:F2}\n";
        desc += $"Cooldown: {stats.cooldown:F2}";

        return desc;
    }
}