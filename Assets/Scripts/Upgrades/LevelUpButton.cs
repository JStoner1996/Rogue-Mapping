using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpButton : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image borderImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite playerUpgradeIcon;

    [Header("UI Elements")]
    public TMP_Text weaponName;
    public Image weaponIcon;
    public TMP_Text rarityText;
    public TMP_Text weaponDescription;

    private Weapon assignedWeapon;
    private WeaponUpgradeResult assignedUpgrade;
    private PlayerStats assignedPlayerStats;
    private PlayerStatUpgradeResult assignedPlayerUpgrade;
    private PlayerLevelUpController playerLevelUpController;

    private WeaponData newWeaponData;
    private bool isNewWeapon;
    private bool isPlayerUpgrade;

    void Start()
    {
        CachePlayerLevelUpController();
    }

    public void ActivateButton(Weapon weapon, WeaponUpgradeResult upgrade)
    {
        // Reset state
        isNewWeapon = false;
        isPlayerUpgrade = false;
        newWeaponData = null;
        assignedPlayerStats = null;
        assignedPlayerUpgrade = null;

        assignedWeapon = weapon;
        assignedUpgrade = upgrade;

        weaponName.text = weapon.Data.weaponName;
        weaponIcon.sprite = weapon.Data.icon;
        weaponIcon.enabled = weapon.Data.icon != null;
        rarityText.text = upgrade.rarity.ToString();

        weaponDescription.text = UpgradeDescriptionFormatter.Build(upgrade);

        ApplyRarityVisuals(upgrade.rarity);
    }

    public void ActivateNewWeaponButton(WeaponData weaponData)
    {
        // Set new weapon mode
        isNewWeapon = true;
        isPlayerUpgrade = false;
        newWeaponData = weaponData;
        assignedPlayerStats = null;
        assignedPlayerUpgrade = null;

        assignedWeapon = null;
        assignedUpgrade = null;

        weaponName.text = weaponData.weaponName;
        weaponIcon.sprite = weaponData.icon;
        weaponIcon.enabled = weaponData.icon != null;

        rarityText.text = "New Weapon";
        weaponDescription.text = "Unlock this weapon";

        ApplyRarityVisuals(UpgradeRarity.Legendary);
    }

    public void ActivatePlayerStatButton(PlayerStats playerStats, PlayerStatUpgradeResult upgrade)
    {
        isNewWeapon = false;
        isPlayerUpgrade = true;
        newWeaponData = null;

        assignedWeapon = null;
        assignedUpgrade = null;
        assignedPlayerStats = playerStats;
        assignedPlayerUpgrade = upgrade;

        weaponName.text = "Player Upgrade";
        weaponIcon.sprite = playerUpgradeIcon;
        weaponIcon.enabled = playerUpgradeIcon != null;
        rarityText.text = upgrade.rarity.ToString();
        weaponDescription.text = PlayerUpgradeDescriptionFormatter.Build(upgrade);

        ApplyRarityVisuals(upgrade.rarity);
    }

    public void SelectUpgrade()
    {
        if (isNewWeapon && newWeaponData != null)
        {
            WeaponController.Instance.AddWeapon(newWeaponData);
        }
        else if (isPlayerUpgrade && assignedPlayerStats != null && assignedPlayerUpgrade != null)
        {
            assignedPlayerStats.ApplyUpgrade(assignedPlayerUpgrade);
        }
        else if (assignedWeapon != null && assignedUpgrade != null)
        {
            assignedWeapon.ApplyUpgrade(assignedUpgrade);
        }

        AudioManager.Instance.Play(SoundType.SelectUpgrade);

        UIController.Instance.LevelUpPanelClosed();
        if (playerLevelUpController == null)
        {
            CachePlayerLevelUpController();
        }

        playerLevelUpController.OnUpgradeSelected();
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
            case UpgradeRarity.Legendary:
                borderColor = new Color(0.45f, 0.10f, 0.70f);
                fillColor = new Color(0.70f, 0.40f, 0.90f);
                break;
            default:
                borderColor = Color.white;
                fillColor = Color.gray;
                break;
        }

        borderImage.color = borderColor;
        backgroundImage.color = fillColor;
    }

    private void CachePlayerLevelUpController()
    {
        if (PlayerController.Instance == null)
        {
            return;
        }

        playerLevelUpController = PlayerController.Instance.GetComponent<PlayerLevelUpController>();
    }
}
