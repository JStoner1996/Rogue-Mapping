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

        weaponDescription.text = UpgradeDescriptionFormatter.Build(upgrade);

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

    private void ApplyRarityVisuals(UpgradeRarity rarity)
    {
        Color borderColor;
        Color fillColor;

        switch (rarity)
        {
            case UpgradeRarity.Common:
                borderColor = new Color(0.55f, 0.27f, 0.07f);
                fillColor = new Color(0.72f, 0.45f, 0.20f);
                break;

            case UpgradeRarity.Uncommon:
                borderColor = new Color(0.75f, 0.75f, 0.75f);
                fillColor = new Color(0.90f, 0.90f, 0.90f);
                break;

            case UpgradeRarity.Rare:
                borderColor = new Color(1.00f, 0.84f, 0.00f);
                fillColor = new Color(1.00f, 0.92f, 0.40f);
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