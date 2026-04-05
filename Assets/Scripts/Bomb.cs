using UnityEngine;

public class Bomb : MonoBehaviour, IItem
{
    [Header("Bomb Stats")]
    [SerializeField] private float damage = 50f;

    public void Collect()
    {
        Explode();
    }

    private void Explode()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
