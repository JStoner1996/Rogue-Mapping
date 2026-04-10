using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectedWeaponUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text infoText;

    public void SetWeapon(WeaponData data)
    {
        if (data == null)
        {
            Clear();
            return;
        }

        icon.sprite = data.icon;
        icon.enabled = data.icon != null;

        var stats = data.baseStats;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Title
        sb.AppendLine($"<b>{data.weaponName}</b>");
        sb.AppendLine();

        // Always show
        sb.AppendLine($"Damage: {stats.damage}");
        sb.AppendLine($"Attack Speed: {stats.attackSpeed}");
        sb.AppendLine($"Range: {stats.range}");
        sb.AppendLine($"Knockback: {stats.knockback}");

        // Conditional stats
        if (stats.duration > 0)
            sb.AppendLine($"Duration: {stats.duration}");

        if (stats.cooldown > 0)
            sb.AppendLine($"Cooldown: {stats.cooldown}");

        if (stats.projectileSpeed > 0)
            sb.AppendLine($"Projectile Speed: {stats.projectileSpeed}");

        if (stats.bounceCount > 0)
            sb.AppendLine($"Bounce Count: {stats.bounceCount}");

        infoText.text = sb.ToString();
    }

    public void Clear()
    {
        icon.sprite = null;
        icon.enabled = false;

        infoText.text = "No Weapon Selected";
    }
}