using System.Collections.Generic;
using UnityEngine;

public class WeaponStatsListUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private WeaponStatsUI weaponStatsPrefab;

    private List<Weapon> currentWeapons = new List<Weapon>();

    void Start()
    {
        currentWeapons = WeaponController.Instance.activeWeapons;
    }

    void Update()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        // Clear
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        // Rebuild
        foreach (var weapon in WeaponController.Instance.activeWeapons)
        {
            WeaponStatsUI card = Instantiate(weaponStatsPrefab, container);
            card.Initialize(weapon);
        }
    }
}