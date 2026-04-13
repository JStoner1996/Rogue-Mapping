using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentAffix", menuName = "Equipment/Affix Definition")]
public class EquipmentAffixDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string affixName;
    [SerializeField] private EquipmentAffixType affixType;

    [Header("Restrictions")]
    [SerializeField] private List<EquipmentSlotType> allowedSlots = new List<EquipmentSlotType>();
    [SerializeField, Min(1)] private int minItemTier = 1;
    [SerializeField, Min(1)] private int maxItemTier = 10;

    [Header("Modifiers")]
    [SerializeField] private List<EquipmentModifierDefinition> modifiers = new List<EquipmentModifierDefinition>();

    public string AffixName => affixName;
    public EquipmentAffixType AffixType => affixType;
    public IReadOnlyList<EquipmentSlotType> AllowedSlots => allowedSlots;
    public int MinItemTier => minItemTier;
    public int MaxItemTier => maxItemTier;
    public IReadOnlyList<EquipmentModifierDefinition> Modifiers => modifiers;

    public bool CanRollFor(EquipmentSlotType slotType, int itemTier)
    {
        return itemTier >= minItemTier
            && itemTier <= maxItemTier
            && (allowedSlots == null || allowedSlots.Count == 0 || allowedSlots.Contains(slotType));
    }

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(affixName))
        {
            return false;
        }

        if (maxItemTier < minItemTier || modifiers == null || modifiers.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (!modifiers[i].IsValid())
            {
                return false;
            }
        }

        return true;
    }
}
