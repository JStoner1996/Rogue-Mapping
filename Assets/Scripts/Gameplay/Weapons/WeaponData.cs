using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public Sprite icon;

    [Header("Prefab")]
    public Weapon prefab;
    public GameObject attackPrefab;

    [Header("Stats")]
    public WeaponStats baseStats;

    [Header("Upgrades")]
    public List<StatRoll> upgradeRolls;
    public List<StatType> allowedStats = new List<StatType>();
    public UpgradePreset upgradePreset;
}