using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject destroyEffect;

    private Vector3 direction;

    [Header("Enemy Stats")]
    [SerializeField] private int experienceWorth;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float health;


    [Header("Knockback")]
    [SerializeField, Range(0f, 100f)]
    private float knockbackResistance = 0f;

    private Vector2 knockbackVelocity;
    private float knockbackTimer;

    [Header("Loot")]
    public List<LootItem> lootTable = new List<LootItem>();


    void FixedUpdate()
    {
        if (!PlayerController.Instance.gameObject.activeSelf)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Face the player
        if (PlayerController.Instance.transform.position.x > transform.position.x)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;

        Vector2 finalVelocity;

        // Knockback active
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            finalVelocity = knockbackVelocity;
        }
        else
        {
            direction = (PlayerController.Instance.transform.position - transform.position).normalized;
            finalVelocity = direction * moveSpeed;
        }

        rb.linearVelocity = finalVelocity;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.Instance.TakeDamage(damage);
        }
    }

    public void TakeDamage(float damage, Vector2? hitDirection = null, float knockbackForce = 0f)
    {
        health -= damage;

        DamageNumberController.Instance.CreateNumber(damage, transform.position);
        // Only apply knockback if both are valid
        if (hitDirection.HasValue && knockbackForce > 0f)
        {
            ApplyKnockback(hitDirection.Value, knockbackForce);
        }

        if (health <= 0)
        {
            DestroyEnemy();
        }
    }

    private void DestroyEnemy()
    {
        foreach (LootItem lootItem in lootTable)
        {
            if (Random.Range(0f, 100f) <= lootItem.dropChance)
            {
                InstantiateLoot(lootItem);
            }
        }

        Destroy(gameObject);
        Instantiate(destroyEffect, transform.position, transform.rotation);
        AudioManager.Instance.Play(SoundType.EnemyDie);

    }

    private void ApplyKnockback(Vector2 direction, float force)
    {
        float resistance = Mathf.Clamp01(knockbackResistance / 100f);
        float finalForce = force * (1f - resistance);

        knockbackVelocity = direction.normalized * finalForce;
        knockbackTimer = 0.1f;
    }
    private void InstantiateLoot(LootItem lootItem)
    {
        if (Random.Range(0f, 100f) > lootItem.dropChance) return;

        switch (lootItem.type)
        {
            case LootType.Health:
                HealthPickup health = PickupPools.Instance.GetHealth();
                health.transform.position = transform.position;
                break;
            case LootType.Experience:
                ExpCrystal xp = PickupPools.Instance.GetXP();
                xp.transform.position = transform.position;
                xp.Init(experienceWorth);
                break;

            case LootType.Magnet:
                Magnet magnet = PickupPools.Instance.GetMagnet();
                magnet.transform.position = transform.position;
                break;

            case LootType.Bomb:
                Bomb bomb = PickupPools.Instance.GetBomb();
                bomb.transform.position = transform.position;
                break;
        }
    }
}
