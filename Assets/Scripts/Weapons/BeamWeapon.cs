using UnityEngine;
using System.Collections.Generic;

public class BeamWeapon : Weapon
{
    private float spawnCounter;

    public GameObject prefab;

    public override void ManualUpdate(float deltaTime)
    {
        spawnCounter -= deltaTime;

        if (spawnCounter <= 0f)
        {
            Enemy target = TargetingUtils.FindNearestEnemy(transform.position, stats.Range);

            // Only spawn if a valid target exists
            if (target != null)
            {
                spawnCounter = spawnCounter = 1f / stats.AttackSpeed; ;

                GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity, transform);

                BeamWeaponPrefab beam = obj.GetComponent<BeamWeaponPrefab>();
                beam.Initialize(this);
            }
            else
            {
                // Optional: retry sooner instead of waiting full interval
                spawnCounter = 0.2f;
            }
        }
    }
}
