using UnityEngine;

public abstract class TargetedWeapon : Weapon
{
    protected float spawnCounter;

    public GameObject prefab;

    public override void ManualUpdate(float deltaTime)
    {
        spawnCounter -= deltaTime;

        while (spawnCounter <= 0f)
        {
            Enemy target = TargetingUtils.FindNearestEnemy(transform.position, stats.Range);

            if (target != null)
            {
                Fire(target);
                spawnCounter += 1f / stats.AttackSpeed;
            }
            else
            {
                // Try again shortly
                spawnCounter = 0.2f;
                break;
            }
        }
    }

    protected virtual void Fire(Enemy target)
    {
        GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity, transform);
        InitializeProjectile(obj, target);
    }

    protected abstract void InitializeProjectile(GameObject obj, Enemy target);
}