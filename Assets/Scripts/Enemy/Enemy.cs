using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static event System.Action EnemyKilled;

    [Header("Enemy Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject destroyEffect;
    [SerializeField] private Material rarityOutlineMaterial;

    [Header("Enemy Stats")]
    [SerializeField] private int experienceWorth;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float health;

    [Header("Knockback")]
    [SerializeField, Range(0f, 100f)]
    private float knockbackResistance = 0f;

    [Header("Rarity Visuals")]
    [SerializeField] private Color uncommonOutlineColor = new Color(0.2f, 0.55f, 1f, 1f);
    [SerializeField] private Color rareOutlineColor = new Color(1f, 0.8f, 0.15f, 1f);
    [SerializeField] private float uncommonOutlineThickness = 1.2f;
    [SerializeField] private float rareOutlineThickness = 1.4f;

    private Vector2 knockbackVelocity;
    private float knockbackTimer;
    private Material defaultMaterial;
    private MaterialPropertyBlock rarityPropertyBlock;

    [Header("Loot")]
    public List<LootItem> lootTable = new List<LootItem>();

    public EnemyRarity Rarity { get; private set; } = EnemyRarity.Normal;
    private PlayerController player;
    private PlayerHealth playerHealth;
    private float currentHealth;
    private EnemySpawnContext spawnContext = EnemySpawnContext.Default;


    void Awake()
    {
        EnsureRarityMaterial();
        ResetRuntimeState();
    }

    void OnEnable()
    {
        ResetRuntimeState();
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            CachePlayerReferences();
        }

        if (player == null || !player.gameObject.activeSelf)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Face the player
        if (player.transform.position.x > transform.position.x)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;

        Vector2 finalVelocity;
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            finalVelocity = knockbackVelocity;
        }
        else
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;
            finalVelocity = direction * CurrentMoveSpeed;
        }

        rb.linearVelocity = finalVelocity;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playerHealth == null)
            {
                CachePlayerReferences();
            }

            playerHealth?.TakeDamage(CurrentContactDamage);
        }
    }

    public void TakeDamage(float damage, Vector2? hitDirection = null, float knockbackForce = 0f)
    {
        currentHealth -= damage;

        DamageNumberController.Instance.CreateNumber(damage, transform.position);
        if (hitDirection.HasValue && knockbackForce > 0f)
        {
            ApplyKnockback(hitDirection.Value, knockbackForce);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Initialize(EnemySpawnContext context)
    {
        spawnContext = context;
        Rarity = context.rarity;
        ResetRuntimeState();
    }

    private float CurrentMoveSpeed => moveSpeed * spawnContext.moveSpeedMultiplier;
    private float CurrentContactDamage => damage * spawnContext.damageMultiplier;
    private int CurrentExperienceWorth => Mathf.RoundToInt(experienceWorth * spawnContext.experienceMultiplier);

    private void Die()
    {
        DropLoot();
        EnemyKilled?.Invoke();

        Destroy(gameObject);
        Instantiate(destroyEffect, transform.position, transform.rotation);
        AudioManager.Instance.Play(SoundType.EnemyDie);
    }

    private void DropLoot()
    {
        foreach (LootItem lootItem in lootTable)
        {
            float dropChance = lootItem.dropChance * spawnContext.dropChanceMultiplier;

            if (Random.Range(0f, 100f) > dropChance)
            {
                continue;
            }

            SpawnLoot(lootItem);
        }
    }

    private void ApplyKnockback(Vector2 hitDirection, float force)
    {
        float resistance = Mathf.Clamp01(knockbackResistance / 100f);
        float finalForce = force * (1f - resistance);

        knockbackVelocity = hitDirection.normalized * finalForce;
        knockbackTimer = 0.1f;
    }

    private void SpawnLoot(LootItem lootItem)
    {
        switch (lootItem.type)
        {
            case LootType.Health:
                HealthPickup health = PickupPools.Instance.GetHealth();
                health.transform.position = transform.position;
                break;
            case LootType.Experience:
                ExpCrystal xp = PickupPools.Instance.GetXP();
                xp.transform.position = transform.position;
                xp.Init(CurrentExperienceWorth);
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

    private void CachePlayerReferences()
    {
        player = PlayerController.Instance;

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void ResetRuntimeState()
    {
        currentHealth = health * spawnContext.healthMultiplier;
        knockbackTimer = 0f;
        knockbackVelocity = Vector2.zero;
        ApplyRarityVisuals();
    }

    private void EnsureRarityMaterial()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        defaultMaterial = spriteRenderer.sharedMaterial;
        rarityPropertyBlock ??= new MaterialPropertyBlock();

        if (rarityOutlineMaterial != null)
        {
            return;
        }

        Shader outlineShader = Shader.Find("Custom/SpriteRarityOutline");

        if (outlineShader != null)
        {
            rarityOutlineMaterial = new Material(outlineShader);
        }
    }

    private void ApplyRarityVisuals()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Color outlineColor = Color.white;
        float outlineThickness = 0f;

        switch (Rarity)
        {
            case EnemyRarity.Uncommon:
                outlineColor = uncommonOutlineColor;
                outlineThickness = uncommonOutlineThickness;
                break;

            case EnemyRarity.Rare:
                outlineColor = rareOutlineColor;
                outlineThickness = rareOutlineThickness;
                break;
        }

        if (rarityOutlineMaterial == null || outlineThickness <= 0f)
        {
            spriteRenderer.sharedMaterial = defaultMaterial;
            spriteRenderer.SetPropertyBlock(null);
            return;
        }

        spriteRenderer.sharedMaterial = rarityOutlineMaterial;
        rarityPropertyBlock.Clear();
        rarityPropertyBlock.SetColor("_OutlineColor", outlineColor);
        rarityPropertyBlock.SetFloat("_OutlineThickness", outlineThickness);
        spriteRenderer.SetPropertyBlock(rarityPropertyBlock);
    }
}
