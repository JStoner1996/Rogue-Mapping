using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Starting Weapons")]
    [SerializeField] private List<Weapon> startingWeapons;

    private List<Weapon> activeWeapons = new List<Weapon>();

    void Start()
    {
        foreach (var weaponPrefab in startingWeapons)
        {
            AddWeapon(weaponPrefab);
        }
    }

    void Update()
    {
        foreach (var weapon in activeWeapons)
        {
            weapon.Tick(Time.deltaTime);
        }
    }

    public void AddWeapon(Weapon weaponPrefab)
    {
        Weapon weapon = Instantiate(weaponPrefab, transform);
        activeWeapons.Add(weapon);
    }

    public void LevelUpWeapon(Weapon weapon)
    {
        weapon.LevelUp();
    }
}