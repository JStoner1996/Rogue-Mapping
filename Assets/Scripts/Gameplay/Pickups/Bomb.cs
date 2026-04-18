using UnityEngine;

public class Bomb : MonoBehaviour, IItem
{
    [Header("Bomb Stats")]
    [SerializeField] private float damage = 50f;

    public void Collect()
    {
        Explode();
        AudioManager.Instance.Play(SoundType.Bomb);
    }

    private void Explode()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            enemy.TakeDamage(damage);
        }

        PickupPools.Instance.ReturnBomb(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }
}
