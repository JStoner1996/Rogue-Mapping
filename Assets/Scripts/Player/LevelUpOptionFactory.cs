using System.Collections.Generic;
using UnityEngine;

public static class LevelUpOptionFactory
{
    public static LevelUpOptionData CreateNewWeaponOption(WeaponData weaponData)
    {
        return new LevelUpOptionData
        {
            optionType = LevelUpOptionType.NewWeapon,
            title = weaponData.weaponName,
            icon = weaponData.icon,
            rarityLabel = "New Weapon",
            description = "Unlock this weapon",
            rarity = UpgradeRarity.Legendary,
            newWeaponData = weaponData,
        };
    }

    public static LevelUpOptionData CreateWeaponUpgradeOption(Weapon weapon, WeaponUpgradeResult upgrade)
    {
        return new LevelUpOptionData
        {
            optionType = LevelUpOptionType.WeaponUpgrade,
            title = weapon.Data.weaponName,
            icon = weapon.Data.icon,
            rarityLabel = upgrade.rarity.ToString(),
            description = UpgradeDescriptionFormatter.Build(upgrade),
            rarity = upgrade.rarity,
            weapon = weapon,
            weaponUpgrade = upgrade,
        };
    }

    public static LevelUpOptionData CreatePlayerUpgradeOption(PlayerStats playerStats, PlayerStatUpgradeResult upgrade, Sprite icon)
    {
        return new LevelUpOptionData
        {
            optionType = LevelUpOptionType.PlayerUpgrade,
            title = "Player Upgrade",
            icon = icon,
            rarityLabel = upgrade.rarity.ToString(),
            description = PlayerUpgradeDescriptionFormatter.Build(upgrade),
            rarity = upgrade.rarity,
            playerStats = playerStats,
            playerUpgrade = upgrade,
        };
    }

    public static List<LevelUpOptionData> BuildOptions(
        WeaponController weaponController,
        PlayerStats playerStats,
        List<WeaponData> allWeapons,
        Sprite playerUpgradeIcon)
    {
        List<LevelUpOptionData> options = new List<LevelUpOptionData>();
        List<Weapon> weapons = weaponController.activeWeapons;
        List<WeaponData> availableWeapons = GetAvailableWeapons(weaponController, allWeapons);
        HashSet<Weapon> usedWeapons = new HashSet<Weapon>();
        HashSet<WeaponData> offeredNewWeapons = new HashSet<WeaponData>();
        HashSet<PlayerStatType> offeredPlayerStats = new HashSet<PlayerStatType>();

        int optionCount = UIController.Instance.GetLevelUpButtons().Length;

        for (int i = 0; i < optionCount; i++)
        {
            LevelUpOptionData option = TryBuildNewWeaponOption(weaponController, availableWeapons, offeredNewWeapons)
                ?? TryBuildPlayerUpgradeOption(playerStats, offeredPlayerStats, playerUpgradeIcon)
                ?? TryBuildWeaponUpgradeOption(weapons, usedWeapons);

            if (option != null)
            {
                options.Add(option);
            }
        }

        return options;
    }

    private static LevelUpOptionData TryBuildNewWeaponOption(
        WeaponController weaponController,
        List<WeaponData> availableWeapons,
        HashSet<WeaponData> offeredNewWeapons)
    {
        bool canAddWeapon = weaponController.CanAddWeapon();
        bool offerNewWeapon = canAddWeapon && Random.value < 0.25f;

        if (!offerNewWeapon)
        {
            return null;
        }

        availableWeapons.RemoveAll(weapon => offeredNewWeapons.Contains(weapon));

        if (availableWeapons.Count == 0)
        {
            return null;
        }

        WeaponData newWeapon = availableWeapons[Random.Range(0, availableWeapons.Count)];
        offeredNewWeapons.Add(newWeapon);
        return CreateNewWeaponOption(newWeapon);
    }

    private static LevelUpOptionData TryBuildPlayerUpgradeOption(
        PlayerStats playerStats,
        HashSet<PlayerStatType> offeredPlayerStats,
        Sprite playerUpgradeIcon)
    {
        bool offerPlayerUpgrade = playerStats != null && Random.value < 0.35f;

        if (!offerPlayerUpgrade)
        {
            return null;
        }

        PlayerStatUpgradeResult playerUpgrade = RollPlayerUpgrade(playerStats, offeredPlayerStats);

        if (playerUpgrade == null)
        {
            return null;
        }

        foreach (PlayerStatType statType in playerUpgrade.stats.Keys)
        {
            offeredPlayerStats.Add(statType);
        }

        return CreatePlayerUpgradeOption(playerStats, playerUpgrade, playerUpgradeIcon);
    }

    private static LevelUpOptionData TryBuildWeaponUpgradeOption(List<Weapon> weapons, HashSet<Weapon> usedWeapons)
    {
        if (weapons.Count == 0)
        {
            return null;
        }

        Weapon selectedWeapon = GetRandomUniqueWeapon(weapons, usedWeapons) ?? weapons[Random.Range(0, weapons.Count)];
        usedWeapons.Add(selectedWeapon);

        if (selectedWeapon.Data.upgradePreset == null)
        {
            Debug.LogError($"UpgradePreset missing on {selectedWeapon.name}");
            return null;
        }

        List<StatRoll> allRolls = selectedWeapon.Data.upgradePreset.rolls;
        HashSet<StatType> allowed = GetAllowedStats(selectedWeapon);
        List<StatRoll> filteredRolls = UpgradeCalculator.FilterRolls(allRolls, allowed);

        if (filteredRolls.Count == 0)
        {
            filteredRolls = allRolls;
        }

        UpgradeRarity rarity = UpgradeCalculator.RollRarity();
        WeaponUpgradeResult upgrade = UpgradeCalculator.RollUpgrade(filteredRolls, rarity);
        return CreateWeaponUpgradeOption(selectedWeapon, upgrade);
    }

    private static PlayerStatUpgradeResult RollPlayerUpgrade(PlayerStats playerStats, HashSet<PlayerStatType> alreadyOfferedStats)
    {
        if (playerStats == null || playerStats.UpgradeRolls.Count == 0)
        {
            return null;
        }

        List<PlayerStatRoll> availableRolls = new List<PlayerStatRoll>();

        foreach (PlayerStatRoll roll in playerStats.UpgradeRolls)
        {
            if (!alreadyOfferedStats.Contains(roll.statType))
            {
                availableRolls.Add(roll);
            }
        }

        if (availableRolls.Count == 0)
        {
            return null;
        }

        UpgradeRarity rarity = UpgradeCalculator.RollRarity();
        return PlayerUpgradeCalculator.RollUpgrade(availableRolls, rarity);
    }

    private static HashSet<StatType> GetAllowedStats(Weapon weapon)
    {
        HashSet<StatType> allowed = new HashSet<StatType>(weapon.Data.allowedStats);

        if (weapon.Data.weaponName == "Area Weapon")
        {
            allowed.Remove(StatType.Cooldown);

            if (weapon.stats.duration >= 5f)
            {
                allowed.Remove(StatType.Duration);
            }
        }

        return allowed;
    }

    private static Weapon GetRandomUniqueWeapon(List<Weapon> weapons, HashSet<Weapon> usedWeapons)
    {
        List<Weapon> pool = new List<Weapon>(weapons);
        pool.RemoveAll(weapon => usedWeapons.Contains(weapon));

        if (pool.Count == 0)
        {
            return null;
        }

        return pool[Random.Range(0, pool.Count)];
    }

    private static List<WeaponData> GetAvailableWeapons(WeaponController weaponController, List<WeaponData> allWeapons)
    {
        List<WeaponData> available = new List<WeaponData>();

        foreach (WeaponData weaponData in allWeapons)
        {
            bool alreadyOwned = weaponController.activeWeapons.Exists(weapon => weapon.Data == weaponData);

            if (!alreadyOwned)
            {
                available.Add(weaponData);
            }
        }

        return available;
    }
}
