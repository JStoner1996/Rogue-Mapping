using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int weaponLevel;
    public List<WeaponStats> stats;
    public Sprite weaponIcon;

    public void LevelUp()
    {
        if (weaponLevel < stats.Count - 1)
            weaponLevel++;
    }

}

[System.Serializable]
public class WeaponStats
{
    public float cooldown;
    public float duration;
    public float damage;
    public float range;
    public float attackSpeed;
    public string description;

}