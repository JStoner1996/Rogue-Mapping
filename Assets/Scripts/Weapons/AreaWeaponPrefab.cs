using System.Collections.Generic;
using UnityEngine;

public class AreaWeaponPrefab : MonoBehaviour
{
    private AreaWeapon weapon;

    private Vector3 targetSize;
    private float timer;
    private float counter;

    public List<Enemy> enemiesInRange = new List<Enemy>();

    public void Initialize(AreaWeapon weaponReference)
    {
        weapon = weaponReference;

        var stats = weapon.CurrentStats;

        targetSize = Vector3.one * stats.range;
        transform.localScale = Vector3.zero;
        timer = stats.duration;
        counter = 0f;

        AudioController.Instance.PlayModifiedSound(AudioController.Instance.areaWeaponSpawn);
    }

    void Update()
    {
        if (weapon == null) return;

        var stats = weapon.CurrentStats;

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

        counter -= Time.deltaTime;

        if (counter <= 0f)
        {
            counter = stats.attackSpeed;

            for (int i = enemiesInRange.Count - 1; i >= 0; i--)
            {
                if (enemiesInRange[i] != null)
                {
                    enemiesInRange[i].TakeDamage(stats.damage);
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