using UnityEngine;

public class ChainLightningWeapon : Weapon
{
    private float spawnCounter;

    public GameObject prefab;

    public override void ManualUpdate(float deltaTime)
    {
        spawnCounter -= deltaTime;

        if (spawnCounter <= 0f)
        {
            Enemy target = TargetingUtils.FindNearestEnemy(transform.position, stats.Range);

            if (target != null)
            {
                spawnCounter = 1f / stats.AttackSpeed;

                GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity, transform);

                ChainLightningPrefab chain = obj.GetComponent<ChainLightningPrefab>();
                chain.Initialize(this, target);
            }
            else
            {
                spawnCounter = 0.2f;
            }
        }
    }
}