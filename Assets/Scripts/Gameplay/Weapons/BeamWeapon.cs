using UnityEngine;

public class BeamWeapon : Weapon
{
    private float spawnCounter;


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

                GameObject obj = Instantiate(Data.attackPrefab, transform.position, Quaternion.identity, transform);

                BeamWeaponAttack beam = obj.GetComponent<BeamWeaponAttack>();
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
