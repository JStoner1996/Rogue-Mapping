using UnityEngine;

public class FireballWeaponPrefab : ProjectileBase
{
    private FireballWeapon weapon;

    public void Initialize(FireballWeapon weaponReference, Enemy target)
    {
        weapon = weaponReference;
        this.target = target;
    }

    protected override void OnHit()
    {
        var stats = weapon.stats;

        float explosionRadius = stats.Range * 0.5f; // 1:4 ratio

#if UNITY_EDITOR
        DebugUtils.DrawCircle(transform.position, explosionRadius, Color.red, 1f);
#endif

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Enemy enemy))
            {
                Vector2 direction = (enemy.transform.position - transform.position).normalized;
                enemy.TakeDamage(stats.Damage, direction, stats.Knockback);
            }
        }

        Destroy(gameObject);
    }
}