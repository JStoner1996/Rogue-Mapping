using UnityEngine;

public class LevelUpOptionData
{
    public LevelUpOptionType optionType;
    public string title;
    public Sprite icon;
    public string rarityLabel;
    public string description;
    public UpgradeRarity rarity;

    public Weapon weapon;
    public WeaponUpgradeResult weaponUpgrade;
    public WeaponData newWeaponData;
    public PlayerStats playerStats;
    public PlayerStatUpgradeResult playerUpgrade;
}
