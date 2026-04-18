using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpButton : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image borderImage;
    [SerializeField] private Image backgroundImage;

    [Header("Fallback Icons")]
    [SerializeField] private Sprite playerUpgradeIcon;
    [Header("UI Elements")]
    public TMP_Text weaponName;
    public Image weaponIcon;
    public TMP_Text rarityText;
    public TMP_Text weaponDescription;

    private LevelUpOptionData assignedOption;
    private PlayerLevelUpController playerLevelUpController;

    void Start()
    {
        CachePlayerLevelUpController();
    }

    public void SetOption(LevelUpOptionData option)
    {
        assignedOption = option;
        Sprite optionIcon = ResolveIcon(option);

        weaponName.text = option.title;
        weaponIcon.sprite = optionIcon;
        weaponIcon.enabled = optionIcon != null;
        rarityText.text = option.rarityLabel;
        weaponDescription.text = option.description;

        ApplyRarityVisuals(option.rarity);
    }

    public void ClearOption()
    {
        assignedOption = null;
        weaponName.text = string.Empty;
        weaponIcon.sprite = null;
        weaponIcon.enabled = false;
        rarityText.text = string.Empty;
        weaponDescription.text = string.Empty;
        ApplyRarityVisuals(UpgradeRarity.Common);
    }

    public void SelectUpgrade()
    {
        if (assignedOption == null)
        {
            return;
        }

        switch (assignedOption.optionType)
        {
            case LevelUpOptionType.NewWeapon:
                if (assignedOption.newWeaponData != null)
                {
                    WeaponController.Instance.AddWeapon(assignedOption.newWeaponData);
                }
                break;

            case LevelUpOptionType.PlayerUpgrade:
                if (assignedOption.playerStats != null && assignedOption.playerUpgrade != null)
                {
                    assignedOption.playerStats.ApplyUpgrade(assignedOption.playerUpgrade);
                }
                break;

            case LevelUpOptionType.WeaponUpgrade:
                if (assignedOption.weapon != null && assignedOption.weaponUpgrade != null)
                {
                    assignedOption.weapon.ApplyUpgrade(assignedOption.weaponUpgrade);
                }
                break;
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

    private Sprite ResolveIcon(LevelUpOptionData option)
    {
        if (option.icon != null)
        {
            return option.icon;
        }

        return option.optionType == LevelUpOptionType.PlayerUpgrade
            ? playerUpgradeIcon
            : null;
    }
}
