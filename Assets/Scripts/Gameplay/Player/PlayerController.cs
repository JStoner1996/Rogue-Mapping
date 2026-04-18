using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : SingletonBehaviour<PlayerController>
{
    [Header("Player Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    [Header("Player References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerHealth playerHealthComponent;
    [SerializeField] private PlayerExperience playerExperienceComponent;
    [SerializeField] private PlayerLevelUpController playerLevelUpController;
    [SerializeField] private PlayerCollector playerCollector;
    [SerializeField] private PlayerMagnet playerMagnet;
    [SerializeField] private PlayerStats playerStatsComponent;

    [Header("Player Movement Config")]
    [SerializeField] private InputActionReference move;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Player Experience Config")]
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private int maxLevel = 50;
    [SerializeField] private List<int> levelThresholds = new List<int>();
    [SerializeField] private int startingExperience = 0;

    [Header("Player Health Config")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float immunityDuration;

    [Header("Player Weapons")]
    [SerializeField] private WeaponController weaponController;

    [Header("Available Weapons")]
    [SerializeField] private List<WeaponData> allWeapons;

    public PlayerMovement PlayerMovementComponent => playerMovement;
    public PlayerHealth PlayerHealthComponent => playerHealthComponent;
    public PlayerExperience PlayerExperienceComponent => playerExperienceComponent;
    public PlayerCollector PlayerCollectorComponent => playerCollector;
    public PlayerStats PlayerStatsComponent => playerStatsComponent;

    private void Awake()
    {
        if (!TryInitializeSingleton())
        {
            return;
        }

        CacheCoreReferences();
        EnsureComponents();
        ConfigureComponents();
    }

    private void CacheCoreReferences()
    {
        rb ??= GetComponent<Rigidbody2D>();
        animator ??= GetComponent<Animator>();
        weaponController ??= GetComponentInChildren<WeaponController>();
    }

    private void EnsureComponents()
    {
        playerMovement = GetOrAddComponent(playerMovement);
        playerHealthComponent = GetOrAddComponent(playerHealthComponent);
        playerExperienceComponent = GetOrAddComponent(playerExperienceComponent);
        playerLevelUpController = GetOrAddComponent(playerLevelUpController);
        playerCollector = GetOrAddComponent(playerCollector);
        playerMagnet = GetOrAddComponent(playerMagnet);
        playerStatsComponent = GetOrAddComponent(playerStatsComponent);
    }

    private void ConfigureComponents()
    {
        if (weaponController == null)
        {
            Debug.LogError("PlayerController requires a WeaponController reference.");
            return;
        }

        playerMovement.Configure(rb, animator, move, moveSpeed);
        playerHealthComponent.Configure(maxHealth, immunityDuration);
        playerExperienceComponent.Configure(startingLevel, maxLevel, levelThresholds, startingExperience);
        playerStatsComponent.Configure(playerMovement, playerHealthComponent, playerCollector, weaponController);
        playerStatsComponent.ApplyEquipmentSummary(MetaProgressionService.GetEquippedEquipmentStatSummary());
        weaponController.Configure(playerStatsComponent);
        playerLevelUpController.Configure(playerExperienceComponent, weaponController, playerStatsComponent, allWeapons);
        playerLevelUpController.RebindExperience();
    }

    private T GetOrAddComponent<T>(T existingComponent) where T : Component
    {
        if (existingComponent != null)
        {
            return existingComponent;
        }

        if (TryGetComponent(out T component))
        {
            return component;
        }

        // Player sub-systems are colocated by design, so missing helpers are safe to add on the fly.
        return gameObject.AddComponent<T>();
    }
}
