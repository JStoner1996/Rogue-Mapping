using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentBase", menuName = "Equipment/Base Definition")]
public class EquipmentBaseDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string baseName;
    [SerializeField] private EquipmentSlotType slotType;
    [SerializeField, Min(1)] private int minItemTier = 1;
    [SerializeField, Min(1)] private int maxItemTier = 10;
    [SerializeField] private Sprite icon;

    [Header("Implicit Modifiers")]
    [SerializeField] private List<EquipmentModifierDefinition> implicitModifiers = new List<EquipmentModifierDefinition>();

    public string BaseName => baseName;
    public EquipmentSlotType SlotType => slotType;
    public int MinItemTier => minItemTier;
    public int MaxItemTier => maxItemTier;
    public Sprite Icon => icon;
    public IReadOnlyList<EquipmentModifierDefinition> ImplicitModifiers => implicitModifiers;

    public bool CanRollAtTier(int itemTier)
    {
        return itemTier >= minItemTier && itemTier <= maxItemTier;
    }

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(baseName))
        {
            return false;
        }

        if (maxItemTier < minItemTier || implicitModifiers == null || implicitModifiers.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < implicitModifiers.Count; i++)
        {
            if (!implicitModifiers[i].IsValid())
            {
                return false;
            }
        }

        return true;
    }
}
