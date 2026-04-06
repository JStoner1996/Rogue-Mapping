using System.Collections.Generic;
using UnityEngine;

public class ChainLightningPrefab : MonoBehaviour
{
    private ChainLightningWeapon weapon;

    private Enemy currentTarget;
    private int remainingBounces;

    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    public float speed = 20f;


    public void Initialize(ChainLightningWeapon weaponReference, Enemy firstTarget)
    {
        weapon = weaponReference;
        var stats = weapon.stats;


        currentTarget = firstTarget;
        remainingBounces = stats.bounceCount;

        hitEnemies.Clear();
    }

    void Update()
    {
        if (currentTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move toward current target
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget.transform.position,
            speed * Time.deltaTime
        );

        // Check if reached target
        if (Vector3.Distance(transform.position, currentTarget.transform.position) < 0.1f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {

        var stats = weapon.stats;
        if (currentTarget == null) return;

        // Damage once per enemy
        if (!hitEnemies.Contains(currentTarget))
        {
            hitEnemies.Add(currentTarget);
            currentTarget.TakeDamage(stats.damage);
        }

        Debug.Log("Remaining bounces: " + remainingBounces);
        remainingBounces--;

        if (remainingBounces <= 0)
        {
            Destroy(gameObject);
            return;
        }

        // Find next target
        Enemy next = TargetingUtils.FindNearestEnemy(currentTarget.transform.position, stats.Range, hitEnemies);

        if (next != null && !hitEnemies.Contains(next))
        {
            Debug.Log("Next target found: " + next.name);
            currentTarget = next;
        }
        else
        {
            Debug.Log("No valid next target found. Destroying chain.");

            Destroy(gameObject);
        }
    }
}