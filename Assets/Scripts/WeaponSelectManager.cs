using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WeaponSelectManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform buttonParent;
    [SerializeField] private WeaponButtonUI buttonPrefab;

    [Header("Data")]
    private List<WeaponData> allWeapons;
    private WeaponData selectedWeapon;

    void Start()
    {
        LoadWeapons();
        CreateButtons();
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
            button.Setup(weapon, OnWeaponSelected);
        }
    }

    void OnWeaponSelected(WeaponData weapon)
    {
        selectedWeapon = weapon;
        Debug.Log($"Selected: {weapon.weaponName}");
    }

    public void ConfirmSelection()
    {
        if (selectedWeapon == null)
        {
            Debug.LogWarning("No weapon selected!");
            return;
        }

        RunData.SelectedWeapon = selectedWeapon;

        SceneManager.LoadScene("Game");
    }
}