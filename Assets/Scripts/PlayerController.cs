using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("Player Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;

    [Header("Player Movement")]
    public Vector3 _moveDirection;
    public InputActionReference move;

    [Header("Player Levels")]
    public int currentLevel;
    public int maxLevel;
    public List<int> playerLevels;
    public int experience;

    [Header("Player Stats")]
    public float playerMaxHealth;
    public float playerHealth;
    public float moveSpeed;

    [Header("Player Weapons")]
    [SerializeField] private WeaponController weaponController;

    [Header("Immunity Handling")]
    private bool isImmune;
    [SerializeField] private float immunityDuration;
    private float immunityTimer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        GetExperienceCurve();
        playerHealth = playerMaxHealth;

        UIController.Instance.UpdateHealthSlider();
        UIController.Instance.UpdateExperienceSlider();
    }

    private void Update()
    {
        _moveDirection = move.action.ReadValue<Vector2>().normalized;

        animator.SetFloat("moveX", _moveDirection.x);
        animator.SetFloat("moveY", _moveDirection.y);
        animator.SetBool("moving", _moveDirection != Vector3.zero);

        if (immunityTimer > 0)
        {
            immunityTimer -= Time.deltaTime;
        }
        else
        {
            isImmune = false;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            _moveDirection.x * moveSpeed,
            _moveDirection.y * moveSpeed
        );
    }

    private void OnEnable()
    {
        ExpCrystal.onExpCrystalCollect += GetExperience;
        HealthPickup.onHealthPickup += GainHealth;

    }

    private void OnDisable()
    {
        ExpCrystal.onExpCrystalCollect -= GetExperience;
        HealthPickup.onHealthPickup -= GainHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isImmune) return;

        isImmune = true;
        immunityTimer = immunityDuration;

        playerHealth -= damage;

        UIController.Instance.UpdateHealthSlider();

        if (playerHealth <= 0)
        {
            gameObject.SetActive(false);
            GameManager.Instance.GameOver();
        }
    }

    public void GetExperience(int addedExperience)
    {
        experience += addedExperience;
        UIController.Instance.UpdateExperienceSlider();

        if (experience >= playerLevels[currentLevel - 1])
        {
            LevelUp();
        }
    }

    private void GetExperienceCurve()
    {
        for (int i = playerLevels.Count; i < maxLevel; i++)
        {
            playerLevels.Add(
                Mathf.CeilToInt(playerLevels[playerLevels.Count - 1] * 1.1f + 15)
            );
        }
    }

    private void GainHealth(int healthAmount)
    {
        playerHealth += healthAmount;
        if (playerHealth > playerMaxHealth)
        {
            playerHealth = playerMaxHealth;
        }

        UIController.Instance.UpdateHealthSlider();
    }

    public void LevelUp()
    {
        experience -= playerLevels[currentLevel - 1];
        currentLevel++;

        AudioController.Instance.PlaySound(AudioController.Instance.levelUp);

        var weapons = weaponController.activeWeapons;

        for (int i = 0; i < UIController.Instance.levelUpButtons.Length; i++)
        {
            if (weapons.Count == 0) continue;

            // Pick a random weapon
            Weapon selectedWeapon = weapons[Random.Range(0, weapons.Count)];

            // Generate a random upgrade for that weapon
            if (selectedWeapon.data.upgradePreset == null)
            {
                Debug.LogError("UpgradePreset is missing on WeaponData!");
                return;
            }

            var rolls = selectedWeapon.data.upgradePreset.rolls;


            UpgradeRarity rarity = UpgradeCalculator.RollRarity();

            WeaponUpgradeResult upgrade = UpgradeCalculator.RollUpgrade(rolls, rarity);

            // Assign to UI
            UIController.Instance.levelUpButtons[i].ActivateButton(selectedWeapon, upgrade);
        }

        UIController.Instance.UpdateExperienceSlider();
        UIController.Instance.LevelUpPanelOpen();
    }
}