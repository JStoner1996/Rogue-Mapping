using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpButton : MonoBehaviour
{
    public TMP_Text weaponName;
    public TMP_Text weaponDescription;
    public Image weaponIcon;

    private Weapon assignedWeapon;

    public void ActivateButton(Weapon weapon)
    {
        weaponName.text = weapon.data.weaponName;
        weaponDescription.text = weapon.CurrentStats.description;
        weaponIcon.sprite = weapon.data.icon;

        assignedWeapon = weapon;
    }

    public void SelectUpgrade()
    {
        assignedWeapon.LevelUp();
        AudioController.Instance.PlaySound(AudioController.Instance.selectUpgrade);
        UIController.Instance.LevelUpPanelClosed();
    }
}
