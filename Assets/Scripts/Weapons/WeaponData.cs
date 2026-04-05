using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public Sprite icon;

    public WeaponStats baseStats;

    public List<StatRoll> upgradeRolls;

    public UpgradePreset upgradePreset;
}