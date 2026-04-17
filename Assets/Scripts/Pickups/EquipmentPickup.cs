using UnityEngine;

// A pickupable equipment item that goes into run loot when collected.
public class EquipmentPickup : MonoBehaviour, IItem
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer rarityRenderer;
    [SerializeField] private SpriteRenderer iconRenderer;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = new Color(0.55f, 0.27f, 0.07f);
    [SerializeField] private Color uncommonColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color rareColor = new Color(1.00f, 0.84f, 0.00f);

    [Header("Audio")]
    [SerializeField] private SoundType dropSound = SoundType.EquipmentDrop;
    [SerializeField] private SoundType pickupSound = SoundType.EquipmentPickup;

    private EquipmentInstance equipment;
    private Sprite defaultIconSprite;
    private Color defaultRarityColor = Color.white;

    void Awake()
    {
        if (iconRenderer != null)
        {
            defaultIconSprite = iconRenderer.sprite;
        }

        if (rarityRenderer != null)
        {
            defaultRarityColor = rarityRenderer.color;
        }
    }

    public void Initialize(EquipmentInstance droppedEquipment)
    {
        equipment = droppedEquipment;

        if (iconRenderer != null)
        {
            iconRenderer.sprite = equipment != null ? equipment.Icon : defaultIconSprite;
            iconRenderer.enabled = iconRenderer.sprite != null;
        }

        if (rarityRenderer != null)
        {
            rarityRenderer.color = GetRarityColor(equipment);
        }

        AudioManager.Instance?.Play(dropSound);
    }

    public void Collect()
    {
        if (equipment == null)
        {
            PickupPools.Instance.ReturnEquipmentPickup(this);
            return;
        }

        Debug.Log($"Picked up equipment: {equipment.DisplayName}\n{EquipmentDescriptionFormatter.BuildStats(equipment)}");
        RunLootService.AddEquipment(equipment);
        AudioManager.Instance?.Play(pickupSound);
        PickupPools.Instance.ReturnEquipmentPickup(this);
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
        equipment = null;

        if (iconRenderer != null)
        {
            iconRenderer.sprite = defaultIconSprite;
            iconRenderer.enabled = iconRenderer.sprite != null;
        }

        if (rarityRenderer != null)
        {
            rarityRenderer.color = defaultRarityColor;
        }
    }

    private Color GetRarityColor(EquipmentInstance droppedEquipment)
    {
        if (droppedEquipment == null)
        {
            return defaultRarityColor;
        }

        return droppedEquipment.Rarity switch
        {
            EquipmentRarity.Common => commonColor,
            EquipmentRarity.Uncommon => uncommonColor,
            EquipmentRarity.Rare => rareColor,
            _ => defaultRarityColor,
        };
    }
}
