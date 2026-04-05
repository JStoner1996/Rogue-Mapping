using System.Collections.Generic;
using Unity.VisualScripting;
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
    public Weapon activeWeapon;

    [Header("Immunity Handling")]
    private bool isImmune;
    [SerializeField] private float immunityDuration;
    [SerializeField] private float immunityTimer;

    [Header("Collector Stats")]
    [SerializeField] private CircleCollider2D collectorCollider;
    [SerializeField] private float pickupRadius;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        Instance = this;
    }

    void Start()
    {
        GetExperienceCurve();
        playerHealth = playerMaxHealth;

        UIController.Instance.UpdateHealthSlider();
        UIController.Instance.UpdateExperienceSlider();

        collectorCollider.radius = pickupRadius;
    }

    private void Update()
    {

        _moveDirection = move.action.ReadValue<Vector2>().normalized;
        animator.SetFloat("moveX", _moveDirection.x);
        animator.SetFloat("moveY", _moveDirection.y);

        if (_moveDirection == Vector3.zero)
        {
            animator.SetBool("moving", false);
        }
        else
        {
            animator.SetBool("moving", true);
        }

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
        rb.linearVelocity = new Vector3(_moveDirection.x * moveSpeed, _moveDirection.y * moveSpeed);
    }

    public void TakeDamage(float damage)
    {
        if (!isImmune)
        {
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
            playerLevels.Add(Mathf.CeilToInt(playerLevels[playerLevels.Count - 1] * 1.1f + 15));
        }
    }

    public void LevelUp()
    {
        experience -= playerLevels[currentLevel - 1];
        currentLevel++;

        AudioController.Instance.PlaySound(AudioController.Instance.levelUp);

        UIController.Instance.UpdateExperienceSlider();
        UIController.Instance.levelUpButtons[0].ActivateButton(activeWeapon);
        UIController.Instance.LevelUpPanelOpen();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        IItem item = collider.GetComponent<IItem>();
        item?.Collect();
    }
}