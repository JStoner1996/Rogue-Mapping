using UnityEngine;

public class MapPickup : MonoBehaviour, IItem
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SoundType dropSound = SoundType.MapDrop;
    [SerializeField] private SoundType pickupSound = SoundType.MapPickup;

    private MapInstance map;

    public void Initialize(MapInstance droppedMap)
    {
        map = droppedMap;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = map != null ? map.Icon : null;
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
            Destroy(gameObject);
            return;
        }

        RunLootService.AddMap(map);
        AudioManager.Instance.Play(pickupSound);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }
}
