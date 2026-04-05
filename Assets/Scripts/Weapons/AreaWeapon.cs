using UnityEngine;

public class AreaWeapon : Weapon
{
    private float spawnCounter;

    public GameObject prefab;

    public override void ManualUpdate(float deltaTime)
    {
        spawnCounter -= deltaTime;

        if (spawnCounter <= 0f)
        {
            spawnCounter = stats.cooldown;

            GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity, transform);

            AreaWeaponPrefab area = obj.GetComponent<AreaWeaponPrefab>();
            area.Initialize(this);
        }
    }
}