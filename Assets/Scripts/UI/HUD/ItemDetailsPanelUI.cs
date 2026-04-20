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
        ShowWithFallback(
            data,
            "No Weapon Selected",
            weapon => ShowContent(weapon.weaponName, weapon.icon, BuildWeaponStats(weapon.baseStats)));
    }

    public void ShowMap(MapInstance map)
    {
        ShowWithFallback(
            map,
            "No Map Selected",
            selectedMap => ShowContent(selectedMap.DisplayName, selectedMap.Icon, MapDescriptionFormatter.BuildStats(selectedMap)));
    }

    public void ShowEquipment()
    {
        ShowPanel();
        ShowContent("Character Equipment", null, "Please hover or click on equipment to see details here.");
    }

    public void ShowEquipment(EquipmentInstance equipment)
    {
        ShowWithFallback(
            equipment,
            "No Equipment Selected",
            selectedEquipment => ShowContent(selectedEquipment.DisplayName, selectedEquipment.Icon, EquipmentDescriptionFormatter.BuildStats(selectedEquipment)));
    }

    public void ShowPanel()
    {
        SetPanelVisible(true);
        hoverFollower?.BeginFollowing();
    }

    public void HidePanel()
    {
        hoverFollower?.StopFollowing();
        SetPanelVisible(false);
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

    private void SetPanelVisible(bool isVisible)
    {
        if (panelRoot != null) panelRoot.SetActive(isVisible);
    }

    private void ShowWithFallback<T>(T value, string emptyMessage, System.Action<T> showValue) where T : class
    {
        ShowPanel();

        if (value == null)
        {
            ShowEmpty(emptyMessage);
            return;
        }

        showValue?.Invoke(value);
    }
}
