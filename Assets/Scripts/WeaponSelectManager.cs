using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WeaponSelectManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform buttonParent;
    [SerializeField] private WeaponButtonUI buttonPrefab;
    [SerializeField] private SelectedWeaponUI selectedWeaponUI;

    [Header("Data")]
    private List<WeaponData> allWeapons;
    private WeaponData selectedWeapon;

    void Start()
    {
        LoadWeapons();
        CreateButtons();

        // Find default weapon
        WeaponData defaultWeapon = allWeapons.Find(w => w.weaponName == "Area Weapon");

        if (defaultWeapon != null)
        {
            OnWeaponSelected(defaultWeapon);
        }
        else
        {
            selectedWeaponUI.Clear();
            Debug.LogWarning("Default weapon not found!");
        }
    }

    void LoadWeapons()
    {
        allWeapons = new List<WeaponData>(
            Resources.LoadAll<WeaponData>("WeaponData")
        );

        Debug.Log($"Loaded {allWeapons.Count} weapons");
    }

    void CreateButtons()
    {
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var weapon in allWeapons)
        {
            WeaponButtonUI button = Instantiate(buttonPrefab, buttonParent);
            button.Setup(weapon, OnWeaponSelected, PreviewWeapon, RestoreSelectedWeaponPreview);
        }
    }

    void OnWeaponSelected(WeaponData weapon)
    {
        selectedWeapon = weapon;
        selectedWeaponUI.SetWeapon(weapon);
        Debug.Log($"Selected: {weapon.weaponName}");
    }

    void PreviewWeapon(WeaponData weapon)
    {
        selectedWeaponUI.SetWeapon(weapon);
    }

    void RestoreSelectedWeaponPreview()
    {
        if (selectedWeapon != null)
        {
            selectedWeaponUI.SetWeapon(selectedWeapon);
            return;
        }

        selectedWeaponUI.Clear();
    }

    public void ConfirmSelection()
    {
        if (selectedWeapon == null)
        {
            Debug.LogWarning("No weapon selected!");
            return;
        }

        RunData.SelectedWeapon = selectedWeapon;
        SceneManager.LoadScene("Map Select");
    }
}
