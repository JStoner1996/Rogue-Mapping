using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{

    public static WeaponController Instance;

    [Header("Starting Weapons")]
    [SerializeField] private List<Weapon> startingWeapons;

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
        foreach (var weaponPrefab in startingWeapons)
        {
            AddWeapon(weaponPrefab);
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

    public void AddWeapon(Weapon weaponPrefab)
    {
        Weapon weapon = Instantiate(weaponPrefab, transform);
        weapon.InitializeStats();

        activeWeapons.Add(weapon);
    }
}