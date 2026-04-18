using UnityEngine;

public class FireballWeaponPrefab : ProjectileBase
{
    private FireballAttack weapon;
    [SerializeField] private GameObject explosionVFX;

    public void Initialize(FireballAttack weaponReference, Enemy target)
    {
        weapon = weaponReference;
        this.target = target;

        projectileSpeed = weapon.stats.projectileSpeed;

        direction = (target.transform.position - transform.position).normalized;
        Rotate(direction, 180f);

        AudioManager.Instance.Play(SoundType.FireballWeapon);
    }
    protected override void OnHit()
    {
        var stats = weapon.stats;

        float explosionRadius = stats.Range * 0.5f;

#if UNITY_EDITOR
        DebugUtils.DrawCircle(transform.position, explosionRadius, Color.red, 1f);
#endif

        GameObject explosion = Instantiate(explosionVFX, transform.position, Quaternion.identity);

        var scaler = explosion.GetComponent<ScaleToRadius>();
        if (scaler != null)
        {
            scaler.SetRadius(explosionRadius);
        }

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