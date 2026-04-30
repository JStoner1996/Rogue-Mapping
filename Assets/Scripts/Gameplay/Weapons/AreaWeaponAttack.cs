using System.Collections.Generic;
using UnityEngine;

public class AreaWeaponAttack : MonoBehaviour
{
    private AreaWeapon weapon;

    private Vector3 targetSize;
    private float timer;
    private float counter;

    public List<Enemy> enemiesInRange = new List<Enemy>();

    public void Initialize(AreaWeapon weaponReference)
    {
        weapon = weaponReference;
        var stats = weapon.stats;

        targetSize = Vector3.one * stats.Range;
        transform.localScale = Vector3.zero;

        timer = stats.duration;
        counter = 0f;

        AudioManager.Instance.Play(SoundType.AreaWeapon);
    }

    void Update()
    {
        if (weapon == null) return;

        var stats = weapon.stats;

        transform.localScale = Vector3.MoveTowards(
            transform.localScale,
            targetSize,
            Time.deltaTime * 5f
        );

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            targetSize = Vector3.zero;

            if (transform.localScale.x <= 0.01f)
            {
                Destroy(gameObject);
            }
        }

        float attackInterval = 1f / stats.AttackSpeed;
        counter -= Time.deltaTime;

        if (counter <= 0f)
        {
            counter = attackInterval;

            for (int i = enemiesInRange.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemiesInRange[i];

                if (enemy != null)
                {
                    Vector2 direction = (enemy.transform.position - transform.position).normalized;

                    enemy.TakeDamage(
                        stats.RollDamage(),
                        direction,
                        stats.Knockback
                    );
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Enemy"))
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && !enemiesInRange.Contains(enemy))
            {
                enemiesInRange.Add(enemy);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Enemy"))
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemiesInRange.Remove(enemy);
            }
        }
    }
}
