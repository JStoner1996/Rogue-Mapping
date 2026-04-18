using UnityEngine;
public class AreaWeapon : Weapon
{
    private float spawnCounter;

    public override void ManualUpdate(float deltaTime)
    {
        spawnCounter -= deltaTime;

        if (spawnCounter <= 0f)
        {
            spawnCounter = stats.cooldown;

            GameObject obj = Instantiate(Data.attackPrefab, transform.position, Quaternion.identity, transform);

            AreaWeaponAttack attack = obj.GetComponent<AreaWeaponAttack>();

            if (attack != null)
            {
                attack.Initialize(this);
            }
            else
            {
                Debug.LogError("AreaWeaponAttack missing on attack prefab!");
            }
        }
    }
}