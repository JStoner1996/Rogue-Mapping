using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpButton : MonoBehaviour
{
    public TMP_Text weaponName;
    public TMP_Text weaponDescription;
    public Image weaponIcon;

    private Weapon assignedWeapon;
    private WeaponUpgradeResult assignedUpgrade;

    // Now we pass BOTH the weapon and the generated upgrade
    public void ActivateButton(Weapon weapon, WeaponUpgradeResult upgrade)
    {
        assignedWeapon = weapon;
        assignedUpgrade = upgrade;

        weaponName.text = weapon.data.weaponName;
        weaponIcon.sprite = weapon.data.icon;

        // Build a description from the upgrade stats
        weaponDescription.text = BuildUpgradeDescription(upgrade);
    }

    public void SelectUpgrade()
    {
        if (assignedWeapon != null && assignedUpgrade != null)
        {
            assignedWeapon.ApplyUpgrade(assignedUpgrade);
        }

        AudioController.Instance.PlaySound(AudioController.Instance.selectUpgrade);
        UIController.Instance.LevelUpPanelClosed();
    }

    private string BuildUpgradeDescription(WeaponUpgradeResult upgrade)
    {
        string desc = "";
        Debug.Log("upgrade:" + upgrade);
        Debug.Log("upgrade stats:" + upgrade.stats);
        foreach (var stat in upgrade.stats)
        {
            desc += $"{stat.Key} +{stat.Value:F2}\n";
        }

        desc += $"\nRarity: {upgrade.rarity}";

        return desc;
    }
}