using System.Collections.Generic;
using UnityEngine;

public class WeaponController : SingletonBehaviour<WeaponController>
{
    [Header("Starting Weapons")]
    [SerializeField] private List<WeaponData> startingWeapons;
    [SerializeField] private PlayerStats playerStats;
    public int maxWeapons = 3;
    public List<Weapon> activeWeapons = new List<Weapon>();

    private void Awake()
    {
        if (!TryInitializeSingleton())
        {
            return;
        }
    }

    private void Start()
    {
        if (RunData.SelectedWeapon != null)
        {
            AddWeapon(RunData.SelectedWeapon);
        }
        else
        {
            foreach (var weaponData in startingWeapons)
            {
                AddWeapon(weaponData);
            }
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        for (int i = 0; i < activeWeapons.Count; i++)
        {
            activeWeapons[i].ManualUpdate(deltaTime);
        }
    }

    public void AddWeapon(WeaponData weaponData)
    {
        if (weaponData == null || weaponData.prefab == null)
        {
            return;
        }

        if (activeWeapons.Count >= maxWeapons)
        {
            Debug.LogWarning("Max weapons reached!");
            return;
        }

        Weapon weapon = Instantiate(weaponData.prefab, transform);
        weapon.Initialize(weaponData);
        playerStats?.ApplyToWeapon(weapon);

        activeWeapons.Add(weapon);
    }

    public bool CanAddWeapon()
    {
        return activeWeapons.Count < maxWeapons;
    }

    public void Configure(PlayerStats configuredPlayerStats)
    {
        playerStats = configuredPlayerStats;
    }

    public void ApplyGlobalPlayerStat(PlayerStatType statType, float value)
    {
        for (int i = 0; i < activeWeapons.Count; i++)
        {
            activeWeapons[i].ApplyGlobalPlayerStat(statType, value);
        }
    }
}
