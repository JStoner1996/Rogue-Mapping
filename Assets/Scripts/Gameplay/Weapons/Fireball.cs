using UnityEngine;

public class FireballAttack : TargetedWeapon
{
    protected override void InitializeProjectile(GameObject obj, Enemy target)
    {
        FireballWeaponPrefab fireball = obj.GetComponent<FireballWeaponPrefab>();
        fireball.Initialize(this, target);
    }
}