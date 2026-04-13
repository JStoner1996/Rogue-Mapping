using UnityEngine;

public class MapPickup : MonoBehaviour, IItem
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SoundType dropSound = SoundType.MapDrop;
    [SerializeField] private SoundType pickupSound = SoundType.MapPickup;

    private MapInstance map;
    private Sprite defaultSprite;

    void Awake()
    {
        if (spriteRenderer != null)
        {
            defaultSprite = spriteRenderer.sprite;
        }
    }

    public void Initialize(MapInstance droppedMap)
    {
        map = droppedMap;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = map != null ? map.Icon : defaultSprite;
            spriteRenderer.enabled = spriteRenderer.sprite != null;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(dropSound);
        }
    }

    public void Collect()
    {
        if (map == null)
        {
            PickupPools.Instance.ReturnMapPickup(this);
            return;
        }

        Debug.Log($"Picked up map: {map.DisplayName}\n{MapDescriptionFormatter.BuildStats(map)}");
        RunLootService.AddMap(map);
        AudioManager.Instance.Play(pickupSound);
        PickupPools.Instance.ReturnMapPickup(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    void OnDisable()
    {
        map = null;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = defaultSprite;
            spriteRenderer.enabled = spriteRenderer.sprite != null;
        }
    }
}
