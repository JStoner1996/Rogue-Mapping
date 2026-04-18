using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerLevelUpController : MonoBehaviour
{
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Sprite playerUpgradeIcon;

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

    private void OnEnable()
    {
        if (playerExperience != null)
        {
            playerExperience.LevelUpAvailable += PresentNextLevelUp;
        }
    }

    private void OnDisable()
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

        AudioManager.Instance?.Play(SoundType.LevelUp);

        List<Weapon> weapons = weaponController.activeWeapons;
        if (weapons.Count == 0)
        {
            return;
        }

        UIController uiController = UIController.Instance;
        if (uiController == null)
        {
            return;
        }

        LevelUpButton[] buttons = uiController.GetLevelUpButtons();
        List<LevelUpOptionData> options = LevelUpOptionFactory.BuildOptions(
            weaponController,
            playerStats,
            allWeapons,
            playerUpgradeIcon
        );

        // Each popup consumes exactly one queued level-up so chained level-ups can present one screen at a time.
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i < options.Count)
            {
                buttons[i].SetOption(options[i]);
                continue;
            }

            buttons[i].ClearOption();
        }

        uiController.LevelUpPanelOpen();
    }
}
