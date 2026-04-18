using System.Collections.Generic;

public class ChainLightningAttack : ProjectileBase
{
    private ChainLightningWeapon weapon;
    private int remainingBounces;
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    public void Initialize(ChainLightningWeapon weaponReference, Enemy firstTarget)
    {
        weapon = weaponReference;
        target = firstTarget;

        projectileSpeed = weapon.stats.projectileSpeed;

        remainingBounces = weapon.stats.bounceCount;
        hitEnemies.Clear();

        AudioManager.Instance.Play(SoundType.ChainLightningWeapon);
    }

    protected override void OnHit()
    {
        var stats = weapon.stats;

        if (!hitEnemies.Contains(target))
        {
            hitEnemies.Add(target);
            target.TakeDamage(stats.Damage);
        }

        remainingBounces--;

        if (remainingBounces <= 0)
        {
            Destroy(gameObject);
            return;
        }

        Enemy next = TargetingUtils.FindNearestEnemy(
            target.transform.position,
            stats.Range,
            hitEnemies
        );

        if (next != null)
        {
            target = next;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}