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

    [Header("Available Weapons")]
    [SerializeField] private List<WeaponData> allWeapons;

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
        LoadAllWeapons();
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

        while (currentLevel < playerLevels.Count && experience >= playerLevels[currentLevel - 1])
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
        playerHealth = Mathf.Min(playerHealth + healthAmount, playerMaxHealth);

        UIController.Instance.UpdateHealthSlider();
    }

    private void LevelUp()
    {
        if (pendingLevelUps <= 0)
            return;

        pendingLevelUps--;

        AudioManager.Instance.Play(SoundType.LevelUp);

        var weapons = weaponController.activeWeapons;
        if (weapons.Count == 0) return;

        var buttons = UIController.Instance.levelUpButtons;

        HashSet<Weapon> usedWeapons = new HashSet<Weapon>();
        HashSet<WeaponData> offeredNewWeapons = new HashSet<WeaponData>();

        List<WeaponData> availableWeapons = GetAvailableWeapons();

        for (int i = 0; i < buttons.Length; i++)
        {
            bool assigned = false;

            bool canAddWeapon = weaponController.CanAddWeapon();
            bool offerNewWeapon = canAddWeapon && Random.value < 0.25f;

            // Offer new weapon with 25% chance if player can carry more, ensuring no duplicates in the offer
            if (offerNewWeapon)
            {
                availableWeapons.RemoveAll(w => offeredNewWeapons.Contains(w));

                if (availableWeapons.Count > 0)
                {
                    WeaponData newWeapon = availableWeapons[Random.Range(0, availableWeapons.Count)];
                    offeredNewWeapons.Add(newWeapon);

                    buttons[i].ActivateNewWeaponButton(newWeapon);
                    assigned = true;
                }
            }


            // Fallback to upgrade if new weapon not offered or unavailable
            if (!assigned)
            {
                // Pick a unique weapon (fallback to any if needed)
                Weapon selectedWeapon = GetRandomUniqueWeapon(weapons, usedWeapons);

                if (selectedWeapon == null)
                {
                    selectedWeapon = weapons[Random.Range(0, weapons.Count)];
                }

                usedWeapons.Add(selectedWeapon);

                if (selectedWeapon.Data.upgradePreset == null)
                {
                    Debug.LogError($"UpgradePreset missing on {selectedWeapon.name}");
                    continue;
                }

                var allRolls = selectedWeapon.Data.upgradePreset.rolls;

                HashSet<StatType> allowed = new HashSet<StatType>(selectedWeapon.Data.allowedStats);

                List<StatRoll> filteredRolls = UpgradeCalculator.FilterRolls(allRolls, allowed);

                // Fallback if filtering removes everything
                if (filteredRolls.Count == 0)
                {
                    filteredRolls = allRolls;
                }

                UpgradeRarity rarity = UpgradeCalculator.RollRarity();

                WeaponUpgradeResult upgrade =
                    UpgradeCalculator.RollUpgrade(filteredRolls, rarity);

                buttons[i].ActivateButton(selectedWeapon, upgrade);
            }
        }

        UIController.Instance.LevelUpPanelOpen();
    }

    private Weapon GetRandomUniqueWeapon(List<Weapon> weapons, HashSet<Weapon> used)
    {
        List<Weapon> pool = new List<Weapon>(weapons);
        pool.RemoveAll(w => used.Contains(w));

        if (pool.Count == 0)
            return null;

        return pool[Random.Range(0, pool.Count)];
    }

    public void OnUpgradeSelected()
    {
        if (pendingLevelUps > 0)
        {
            LevelUp();
        }

    }

    private void LoadAllWeapons()
    {
        allWeapons = new List<WeaponData>(Resources.LoadAll<WeaponData>("WeaponData"));

        Debug.Log($"Loaded {allWeapons.Count} weapons.");
    }

    private List<WeaponData> GetAvailableWeapons()
    {
        List<WeaponData> available = new List<WeaponData>();

        foreach (WeaponData weaponData in allWeapons)
        {
            bool alreadyOwned = weaponController.activeWeapons
                .Exists(w => w.Data == weaponData);

            if (!alreadyOwned)
            {
                available.Add(weaponData);
            }
        }

        return available;
    }
}
