using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        ShowPanel();

        if (map == null)
        {
            Clear("No Map Selected");
            return;
        }

        titleText.text = map.DisplayName;
        icon.sprite = map.Icon;
        icon.enabled = icon.sprite != null;
        statsText.text = MapDescriptionFormatter.BuildStats(map);
    }

    public void ShowEquipment()
    {
        ShowPanel();
        titleText.text = "Character Equipment";
        icon.sprite = null;
        icon.enabled = false;
        statsText.text = "Coming soon.\n\nThis tab is reserved for future character equipment and loadout logic.";
    }

    public void ShowEquipment(EquipmentInstance equipment)
    {
        ShowPanel();

        if (equipment == null)
        {
            Clear("No Equipment Selected");
            return;
        }

        titleText.text = equipment.DisplayName;
        icon.sprite = equipment.Icon;
        icon.enabled = icon.sprite != null;
        statsText.text = EquipmentDescriptionFormatter.BuildStats(equipment);
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
        titleText.text = "Currently Selected";
        icon.sprite = null;
        icon.enabled = false;
        statsText.text = message;
    }
}
