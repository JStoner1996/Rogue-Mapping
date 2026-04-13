using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentIconCatalog", menuName = "Equipment/Icon Catalog")]
public class EquipmentIconCatalog : ScriptableObject
{
    [SerializeField] private Sprite headIcon;
    [SerializeField] private Sprite chestIcon;
    [SerializeField] private Sprite legsIcon;
    [SerializeField] private Sprite feetIcon;
    [SerializeField] private Sprite necklaceIcon;
    [SerializeField] private Sprite handsIcon;
    [SerializeField] private Sprite ringIcon;

    public Sprite GetDefaultIcon(EquipmentSlotType slotType)
    {
        return slotType switch
        {
            EquipmentSlotType.Head => headIcon,
            EquipmentSlotType.Chest => chestIcon,
            EquipmentSlotType.Legs => legsIcon,
            EquipmentSlotType.Feet => feetIcon,
            EquipmentSlotType.Necklace => necklaceIcon,
            EquipmentSlotType.Hands => handsIcon,
            EquipmentSlotType.Ring => ringIcon,
            _ => null
        };
    }
}
