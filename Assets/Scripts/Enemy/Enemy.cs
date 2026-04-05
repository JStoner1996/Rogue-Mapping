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
    [SerializeField] private float pushTime;
    private float pushCounter;

    [Header("Loot")]
    public List<LootItem> lootTable = new List<LootItem>();


    // Update is called once per frame
    void FixedUpdate()
    {
        if (PlayerController.Instance.gameObject.activeSelf)
        {

            // Face the player
            if (PlayerController.Instance.transform.position.x > transform.position.x)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }

            // push back
            if (pushCounter > 0)
            {
                pushCounter -= Time.deltaTime;
                if (moveSpeed > 0)
                {
                    moveSpeed = -moveSpeed;
                }

                if (pushCounter <= 0)
                {
                    moveSpeed = Mathf.Abs(moveSpeed);
                }
            }

            // Move towards player
            direction = (PlayerController.Instance.transform.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, direction.y * moveSpeed);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.Instance.TakeDamage(damage);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        DamageNumberController.Instance.CreateNumber(damage, transform.position);
        pushCounter = pushTime;

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
        AudioController.Instance.PlayModifiedSound(AudioController.Instance.enemyDie);
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
