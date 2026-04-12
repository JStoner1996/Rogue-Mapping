using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerLevelUpController : MonoBehaviour
{
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private PlayerStats playerStats;

    private PlayerExperience playerExperience;
    private List<WeaponData> allWeapons = new List<WeaponData>();
    private bool initialized;

    public void Configure(PlayerExperience experienceComponent, WeaponController configuredWeaponController, PlayerStats configuredPlayerStats, List<WeaponData> configuredWeapons)
    {
        playerExperience = experienceComponent;
        weaponController = configuredWeaponController;
        playerStats = configuredPlayerStats;
        allWeapons = configuredWeapons != null && configuredWeapons.Count > 0
            ? new List<WeaponData>(configuredWeapons)
            : new List<WeaponData>(Resources.LoadAll<WeaponData>("WeaponData"));
        initialized = true;
    }

    void OnEnable()
    {
        if (playerExperience != null)
        {
            playerExperience.LevelUpAvailable += PresentNextLevelUp;
        }
    }

    void OnDisable()
    {
        if (playerExperience != null)
        {
            playerExperience.LevelUpAvailable -= PresentNextLevelUp;
        }
    }

    public void RebindExperience()
    {
        if (playerExperience == null)
        {
            return;
        }

        playerExperience.LevelUpAvailable -= PresentNextLevelUp;
        playerExperience.LevelUpAvailable += PresentNextLevelUp;
    }

    public void OnUpgradeSelected()
    {
        if (playerExperience != null && playerExperience.HasPendingLevelUps)
        {
            PresentNextLevelUp();
        }
    }

    public void PresentNextLevelUp()
    {
        if (!initialized || playerExperience == null || !playerExperience.TryConsumePendingLevelUp())
        {
            return;
        }

        AudioManager.Instance.Play(SoundType.LevelUp);

        List<Weapon> weapons = weaponController.activeWeapons;
        if (weapons.Count == 0)
        {
            return;
        }

        LevelUpButton[] buttons = UIController.Instance.levelUpButtons;
        HashSet<Weapon> usedWeapons = new HashSet<Weapon>();
        HashSet<WeaponData> offeredNewWeapons = new HashSet<WeaponData>();
        HashSet<PlayerStatType> offeredPlayerStats = new HashSet<PlayerStatType>();
        List<WeaponData> availableWeapons = GetAvailableWeapons();

        for (int i = 0; i < buttons.Length; i++)
        {
            bool assigned = false;
            bool canAddWeapon = weaponController.CanAddWeapon();
            bool offerNewWeapon = canAddWeapon && Random.value < 0.25f;
            bool offerPlayerUpgrade = playerStats != null && Random.value < 0.35f;

            if (offerNewWeapon)
            {
                availableWeapons.RemoveAll(w => offeredNewWeapons.Contains(w));

                if (availableWeapons.Count > 0)
                {
                    WeaponData newWeapon = availableWeapons[Random.Range(0, availableWeapons.Count)];
                    offeredNewWeapons.Add(newWeapon);
                    buttons[i].ActivateNewWeaponButton(newWeapon);
                    assigned = true;
                }
            }

            if (assigned)
            {
                continue;
            }

            if (offerPlayerUpgrade)
            {
                PlayerStatUpgradeResult playerUpgrade = RollPlayerUpgrade(offeredPlayerStats);

                if (playerUpgrade != null)
                {
                    foreach (PlayerStatType statType in playerUpgrade.stats.Keys)
                    {
                        offeredPlayerStats.Add(statType);
                    }

                    buttons[i].ActivatePlayerStatButton(playerStats, playerUpgrade);
                    continue;
                }
            }

            Weapon selectedWeapon = GetRandomUniqueWeapon(weapons, usedWeapons) ?? weapons[Random.Range(0, weapons.Count)];
            usedWeapons.Add(selectedWeapon);

            if (selectedWeapon.Data.upgradePreset == null)
            {
                Debug.LogError($"UpgradePreset missing on {selectedWeapon.name}");
                continue;
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
            buttons[i].ActivateButton(selectedWeapon, upgrade);
        }

        UIController.Instance.LevelUpPanelOpen();
    }

    private Weapon GetRandomUniqueWeapon(List<Weapon> weapons, HashSet<Weapon> usedWeapons)
    {
        List<Weapon> pool = new List<Weapon>(weapons);
        pool.RemoveAll(w => usedWeapons.Contains(w));

        if (pool.Count == 0)
        {
            return null;
        }

        return pool[Random.Range(0, pool.Count)];
    }

    private List<WeaponData> GetAvailableWeapons()
    {
        List<WeaponData> available = new List<WeaponData>();

        foreach (WeaponData weaponData in allWeapons)
        {
            bool alreadyOwned = weaponController.activeWeapons.Exists(w => w.Data == weaponData);

            if (!alreadyOwned)
            {
                available.Add(weaponData);
            }
        }

        return available;
    }

    private PlayerStatUpgradeResult RollPlayerUpgrade(HashSet<PlayerStatType> alreadyOfferedStats)
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

    private HashSet<StatType> GetAllowedStats(Weapon weapon)
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
}
