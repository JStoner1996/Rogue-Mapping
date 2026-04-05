using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData data;
    public int weaponLevel;

    protected float cooldownTimer;

    public WeaponStats CurrentStats => data.levels[weaponLevel];

    public virtual void Tick(float deltaTime)
    {
        cooldownTimer -= deltaTime;

        if (cooldownTimer <= 0f)
        {
            cooldownTimer = CurrentStats.cooldown;
            Fire();
        }
    }

    protected virtual void Fire()
    {
        // Override in child weapons
    }

    public virtual void LevelUp()
    {
        if (weaponLevel < data.levels.Count - 1)
            weaponLevel++;
    }
}