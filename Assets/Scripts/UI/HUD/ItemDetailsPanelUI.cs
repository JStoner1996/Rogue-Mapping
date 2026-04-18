using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class ItemDetailsPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private HoverPopupFollowerUI hoverFollower;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text statsText;

    public void ShowWeapon(WeaponData data)
    {
        ShowPanel();

        if (data == null)
        {
            ShowEmpty("No Weapon Selected");
            return;
        }

        ShowContent(data.weaponName, data.icon, BuildWeaponStats(data.baseStats));
    }

    public void ShowMap(MapInstance map)
    {
        ShowPanel();

        if (map == null)
        {
            ShowEmpty("No Map Selected");
            return;
        }

        ShowContent(map.DisplayName, map.Icon, MapDescriptionFormatter.BuildStats(map));
    }

    public void ShowEquipment()
    {
        ShowPanel();
        ShowContent("Character Equipment", null, "Please hover or click on equipment to see details here.");
    }

    public void ShowEquipment(EquipmentInstance equipment)
    {
        ShowPanel();

        if (equipment == null)
        {
            ShowEmpty("No Equipment Selected");
            return;
        }

        ShowContent(equipment.DisplayName, equipment.Icon, EquipmentDescriptionFormatter.BuildStats(equipment));
    }

    public void ShowPanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        hoverFollower?.BeginFollowing();
    }

    public void HidePanel()
    {
        hoverFollower?.StopFollowing();

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void Clear(string message)
    {
        ShowEmpty(message);
    }

    private void ShowEmpty(string message)
    {
        ShowContent("Currently Selected", null, message);
    }

    private void ShowContent(string title, Sprite sprite, string stats)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }

        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        if (statsText != null)
        {
            statsText.text = stats ?? string.Empty;
        }
    }

    private static string BuildWeaponStats(WeaponStats stats)
    {
        if (stats == null)
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Damage: {stats.damage}");
        builder.AppendLine($"Attack Speed: {stats.attackSpeed}");
        builder.AppendLine($"Range: {stats.range}");
        builder.AppendLine($"Knockback: {stats.knockback}");
        AppendOptionalStat(builder, "Duration", stats.duration);
        AppendOptionalStat(builder, "Cooldown", stats.cooldown);
        AppendOptionalStat(builder, "Projectile Speed", stats.projectileSpeed);

        if (stats.bounceCount > 0)
        {
            builder.AppendLine($"Bounce Count: {stats.bounceCount}");
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendOptionalStat(StringBuilder builder, string label, float value)
    {
        if (value > 0f)
        {
            builder.AppendLine($"{label}: {value}");
        }
    }
}
