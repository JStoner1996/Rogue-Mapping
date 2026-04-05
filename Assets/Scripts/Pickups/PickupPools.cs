using UnityEngine;

public class PickupPools : MonoBehaviour
{
    public static PickupPools Instance;

    [Header("Health Pickup")]
    [SerializeField] private HealthPickup healthPrefab;
    [SerializeField] private int healthPoolSize = 3;

    [Header("XP Crystal")]
    [SerializeField] private ExpCrystal xpPrefab;
    [SerializeField] private int xpPoolSize = 150;

    [Header("Magnet")]
    [SerializeField] private Magnet magnetPrefab;
    [SerializeField] private int magnetPoolSize = 3;

    [Header("Bomb")]
    [SerializeField] private Bomb bombPrefab;
    [SerializeField] private int bombPoolSize = 3;


    private ObjectPool<HealthPickup> healthPool;
    private ObjectPool<ExpCrystal> xpPool;
    private ObjectPool<Magnet> magnetPool;
    private ObjectPool<Bomb> bombPool;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        healthPool = new ObjectPool<HealthPickup>(healthPrefab, healthPoolSize);
        xpPool = new ObjectPool<ExpCrystal>(xpPrefab, xpPoolSize);
        magnetPool = new ObjectPool<Magnet>(magnetPrefab, magnetPoolSize);
        bombPool = new ObjectPool<Bomb>(bombPrefab, bombPoolSize);
    }

    // -------- Health Pickup --------
    public HealthPickup GetHealth()
    {
        return healthPool.Get();
    }

    public void ReturnHealth(HealthPickup obj)
    {
        healthPool.ReturnToPool(obj);
    }

    // -------- XP --------
    public ExpCrystal GetXP()
    {
        return xpPool.Get();
    }

    public void ReturnXP(ExpCrystal obj)
    {
        xpPool.ReturnToPool(obj);
    }

    // -------- Magnet --------
    public Magnet GetMagnet()
    {
        return magnetPool.Get();
    }

    public void ReturnMagnet(Magnet obj)
    {
        magnetPool.ReturnToPool(obj);
    }

    // -------- Bomb --------
    public Bomb GetBomb()
    {
        return bombPool.Get();
    }

    public void ReturnBomb(Bomb obj)
    {
        bombPool.ReturnToPool(obj);
    }
}