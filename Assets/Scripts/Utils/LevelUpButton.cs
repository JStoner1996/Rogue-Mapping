using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpButton : MonoBehaviour
{

    [Header("Components")]
    [SerializeField] private Image borderImage;
    [SerializeField] private Image backgroundImage;

    [Header("UI Elements")]
    public TMP_Text weaponName;
    public Image weaponIcon;
    public TMP_Text rarityText;
    public TMP_Text weaponDescription;

    private Weapon assignedWeapon;
    private WeaponUpgradeResult assignedUpgrade;

    public void ActivateButton(Weapon weapon, WeaponUpgradeResult upgrade)
    {
        assignedWeapon = weapon;
        assignedUpgrade = upgrade;

        weaponName.text = weapon.data.weaponName;
        weaponIcon.sprite = weapon.data.icon;

        rarityText.text = upgrade.rarity.ToString();

        // Build a description from the upgrade stats
        weaponDescription.text = BuildUpgradeDescription(upgrade);

        ApplyRarityVisuals(upgrade.rarity);
    }

    public void SelectUpgrade()
    {
        if (assignedWeapon != null && assignedUpgrade != null)
        {
            assignedWeapon.ApplyUpgrade(assignedUpgrade);
        }

        AudioController.Instance.PlaySound(AudioController.Instance.selectUpgrade);
        UIController.Instance.LevelUpPanelClosed();
        PlayerController.Instance.OnUpgradeSelected();
    }

    private string BuildUpgradeDescription(WeaponUpgradeResult upgrade)
    {
        string desc = "";

        foreach (var stat in upgrade.stats)
        {
            desc += FormatStat(stat.Key, stat.Value) + "\n";
        }

        // Debug: Show weight in description for testing purposes
        desc += "\n" + "Weight: " + upgrade.weight.ToString("F2");

        return desc;
    }

    private string FormatStat(StatType statType, float value)
    {
        string statName = StringUtils.SplitCamelCase(statType.ToString());

        switch (statType)
        {
            case StatType.BounceCount:
                return $"{statName} +{Mathf.RoundToInt(value)}";

            case StatType.Damage:
            case StatType.Knockback:
            case StatType.AttackSpeed:
            case StatType.Range:
                float percent = value * 100f;
                return $"{statName} +{percent:F0}%";

            case StatType.Duration:
            case StatType.Cooldown:
                return $"{statName} {value:F2}s";

            default:
                return $"{statName} +{value:F2}";
        }
    }

    private void ApplyRarityVisuals(UpgradeRarity rarity)
    {
        Color borderColor;
        Color fillColor;

        switch (rarity)
        {
            case UpgradeRarity.Common:
                borderColor = new Color(0.55f, 0.27f, 0.07f); // bronze
                fillColor = new Color(0.72f, 0.45f, 0.20f); // lighter bronze
                break;

            case UpgradeRarity.Uncommon:
                borderColor = new Color(0.75f, 0.75f, 0.75f); // silver
                fillColor = new Color(0.90f, 0.90f, 0.90f); // lighter silver
                break;

            case UpgradeRarity.Rare:
                borderColor = new Color(1.00f, 0.84f, 0.00f); // gold
                fillColor = new Color(1.00f, 0.92f, 0.40f); // lighter gold
                break;

            default:
                borderColor = Color.white;
                fillColor = Color.gray;
                break;
        }

        borderImage.color = borderColor;
        backgroundImage.color = fillColor;
    }
}