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

    [Header("Map Pickup")]
    [SerializeField] private MapPickup mapPickupPrefab;
    [SerializeField] private int mapPickupPoolSize = 10;

    [Header("Equipment Pickup")]
    [SerializeField] private EquipmentPickup equipmentPickupPrefab;
    [SerializeField] private int equipmentPickupPoolSize = 10;

    private ObjectPool<HealthPickup> healthPool;
    private ObjectPool<ExpCrystal> xpPool;
    private ObjectPool<Magnet> magnetPool;
    private ObjectPool<Bomb> bombPool;
    private ObjectPool<MapPickup> mapPickupPool;
    private ObjectPool<EquipmentPickup> equipmentPickupPool;

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
        mapPickupPool = new ObjectPool<MapPickup>(mapPickupPrefab, mapPickupPoolSize);
        equipmentPickupPool = new ObjectPool<EquipmentPickup>(equipmentPickupPrefab, equipmentPickupPoolSize);
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

    // -------- Map Pickup --------
    public MapPickup GetMapPickup()
    {
        return mapPickupPool.Get();
    }

    public void ReturnMapPickup(MapPickup obj)
    {
        mapPickupPool.ReturnToPool(obj);
    }

    // -------- Equipment Pickup --------
    public EquipmentPickup GetEquipmentPickup()
    {
        return equipmentPickupPool.Get();
    }

    public void ReturnEquipmentPickup(EquipmentPickup obj)
    {
        equipmentPickupPool.ReturnToPool(obj);
    }
}
