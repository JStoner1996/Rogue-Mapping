using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{

    public static WeaponController Instance;

    [Header("Starting Weapons")]
    [SerializeField] private List<WeaponData> startingWeapons;
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
        foreach (var weaponData in startingWeapons)
        {
            AddWeapon(weaponData);
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

        activeWeapons.Add(weapon);
    }

    public bool CanAddWeapon()
    {
        return activeWeapons.Count < maxWeapons;
    }
}