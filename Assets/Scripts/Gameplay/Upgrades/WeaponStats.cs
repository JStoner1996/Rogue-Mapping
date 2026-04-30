using System;
using UnityEngine;

[Serializable]
public class WeaponStats
{
    public float damage;
    [Range(0f, 1f)] public float criticalChance;
    public float attackSpeed;
    public float range;
    public float knockback;
    public float duration;
    public float cooldown;
    public float projectileSpeed;
    public int bounceCount;
}
