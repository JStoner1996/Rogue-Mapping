using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{

    public static WeaponController Instance;

    [Header("Starting Weapons")]
    [SerializeField] private List<WeaponData> startingWeapons;
    [SerializeField] private PlayerStats playerStats;
    public int maxWeapons = 3;
    public List<Weapon> activeWeapons = new List<Weapon>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
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

    void Update()
    {
        float deltaTime = Time.deltaTime;

        foreach (var weapon in activeWeapons)
        {
            weapon.ManualUpdate(deltaTime);
        }
    }

    public void AddWeapon(WeaponData weaponData)
    {
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
        foreach (Weapon weapon in activeWeapons)
        {
            weapon.ApplyGlobalPlayerStat(statType, value);
        }
    }
}
