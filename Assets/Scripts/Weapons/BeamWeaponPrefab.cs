using System.Collections.Generic;
using UnityEngine;

public class BeamWeaponPrefab : MonoBehaviour
{
    private BeamWeapon weapon;
    private Vector3 targetSize;

    public void Initialize(BeamWeapon weaponReference)
    {
        weapon = weaponReference;
        var stats = weapon.stats;

        Vector3 currentScale = transform.localScale;
        targetSize = new Vector3(currentScale.x, stats.Range, currentScale.z);
        FireBeam(TargetingUtils.FindNearestEnemy(transform.position, stats.Range));
    }

    void Update()
    {
        if (weapon == null) return;

        // Scale Y toward target
        Vector3 scale = transform.localScale;

        scale.y = Mathf.MoveTowards(
            scale.y,
            targetSize.y,
            Time.deltaTime * 80f
        );

        transform.localScale = scale;

        if (scale.y >= targetSize.y * 0.99f)
        {
            Destroy(gameObject, 0.3f);
        }
    }

    private void FireBeam(Enemy target)
    {
        if (target == null) return;

        var stats = weapon.stats;

        Vector3 start = transform.position;
        float range = stats.Range;
        float width = transform.localScale.x;

        Vector3 direction = (target.transform.position - start).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float rotationZ = angle - 90f;

        transform.localRotation = Quaternion.Euler(0, 0, rotationZ);

        Vector2 boxSize = new Vector2(width, range);
        Vector2 boxCenter = (Vector2)start + (Vector2)direction * (range / 2f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, rotationZ);

        HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Enemy enemy))
            {
                if (hitEnemies.Add(enemy))
                {
                    enemy.TakeDamage(stats.Damage, direction, stats.Knockback);
                }
            }
        }
    }

}