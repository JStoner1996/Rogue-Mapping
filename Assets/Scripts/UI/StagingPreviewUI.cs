using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StagingPreviewUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Sprite defaultMapIcon;

    public void ShowWeapon(WeaponData data)
    {
        if (data == null)
        {
            Clear("No Weapon Selected");
            return;
        }

        titleText.text = data.weaponName;
        icon.sprite = data.icon;
        icon.enabled = data.icon != null;

        var stats = data.baseStats;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"Damage: {stats.damage}");
        sb.AppendLine($"Attack Speed: {stats.attackSpeed}");
        sb.AppendLine($"Range: {stats.range}");
        sb.AppendLine($"Knockback: {stats.knockback}");

        if (stats.duration > 0)
            sb.AppendLine($"Duration: {stats.duration}");
        if (stats.cooldown > 0)
            sb.AppendLine($"Cooldown: {stats.cooldown}");
        if (stats.projectileSpeed > 0)
            sb.AppendLine($"Projectile Speed: {stats.projectileSpeed}");
        if (stats.bounceCount > 0)
            sb.AppendLine($"Bounce Count: {stats.bounceCount}");

        statsText.text = sb.ToString().TrimEnd();
    }

    public void ShowMap(MapInstance map)
    {
        if (map == null)
        {
            Clear("No Map Selected");
            return;
        }

        titleText.text = map.DisplayName;
        icon.sprite = map.Icon != null ? map.Icon : defaultMapIcon;
        icon.enabled = icon.sprite != null;
        statsText.text = MapDescriptionFormatter.BuildStats(map);
    }

    public void ShowEquipment()
    {
        titleText.text = "Character Equipment";
        icon.sprite = null;
        icon.enabled = false;
        statsText.text = "Coming soon.\n\nThis tab is reserved for future character equipment and loadout logic.";
    }

    public void Clear(string message)
    {
        titleText.text = "Currently Selected";
        icon.sprite = null;
        icon.enabled = false;
        statsText.text = message;
    }
}
