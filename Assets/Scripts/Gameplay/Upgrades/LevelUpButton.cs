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
    private static readonly (Color border, Color fill)[] RarityColors =
    {
        (new Color(0.55f, 0.27f, 0.07f), new Color(0.72f, 0.45f, 0.20f)),
        (new Color(0.75f, 0.75f, 0.75f), new Color(0.90f, 0.90f, 0.90f)),
        (new Color(1.00f, 0.84f, 0.00f), new Color(1.00f, 0.92f, 0.40f)),
        (new Color(0.45f, 0.10f, 0.70f), new Color(0.70f, 0.40f, 0.90f)),
    };

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
        SetText(weaponName, string.Empty);
        SetIcon(null);
        SetText(rarityText, string.Empty);
        SetText(weaponDescription, string.Empty);
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
        if (playerLevelUpController == null) CachePlayerLevelUpController();
        playerLevelUpController.OnUpgradeSelected();
    }

    private void ApplyRarityVisuals(UpgradeRarity rarity)
    {
        (Color borderColor, Color fillColor) = GetRarityColors(rarity);
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

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null) text.text = value;
    }

    private void SetIcon(Sprite sprite)
    {
        if (weaponIcon == null)
        {
            return;
        }

        weaponIcon.sprite = sprite;
        weaponIcon.enabled = sprite != null;
    }

    private static (Color border, Color fill) GetRarityColors(UpgradeRarity rarity) =>
        (int)rarity >= 0 && (int)rarity < RarityColors.Length ? RarityColors[(int)rarity] : (Color.white, Color.gray);
}
