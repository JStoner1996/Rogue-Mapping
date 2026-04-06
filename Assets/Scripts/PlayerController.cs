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

    private int pendingLevelUps = 0;

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

        while (currentLevel < playerLevels.Count &&
               experience >= playerLevels[currentLevel - 1])
        {
            experience -= playerLevels[currentLevel - 1];
            currentLevel++;
            pendingLevelUps++;
        }

        UIController.Instance.UpdateExperienceSlider();

        if (pendingLevelUps > 0)
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

    private void LevelUp()
    {
        if (pendingLevelUps <= 0)
            return;

        pendingLevelUps--;

        AudioController.Instance.PlaySound(AudioController.Instance.levelUp);

        var weapons = weaponController.activeWeapons;

        if (weapons.Count == 0) return;

        for (int i = 0; i < UIController.Instance.levelUpButtons.Length; i++)
        {
            Weapon selectedWeapon = weapons[Random.Range(0, weapons.Count)];

            if (selectedWeapon.data.upgradePreset == null)
            {
                Debug.LogError("UpgradePreset is missing on WeaponData!");
                continue;
            }

            var rolls = selectedWeapon.data.upgradePreset.rolls;

            UpgradeRarity rarity = UpgradeCalculator.RollRarity();

            WeaponUpgradeResult upgrade =
                UpgradeCalculator.RollUpgrade(rolls, rarity);

            UIController.Instance.levelUpButtons[i]
                .ActivateButton(selectedWeapon, upgrade);
        }

        UIController.Instance.LevelUpPanelOpen();

    }

    public void OnUpgradeSelected()
    {
        if (pendingLevelUps > 0)
        {
            LevelUp();
        }

    }
}