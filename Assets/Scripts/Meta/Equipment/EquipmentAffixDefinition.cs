using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentAffix", menuName = "Equipment/Affix Definition")]
public class EquipmentAffixDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string affixName;
    [SerializeField] private EquipmentAffixType affixType;
    [SerializeField, Min(1)] private int affixTier = 1;
    [SerializeField] private string affixTag;

    [Header("Restrictions")]
    [SerializeField] private List<EquipmentSlotType> allowedSlots = new List<EquipmentSlotType>();
    [SerializeField, Min(1)] private int minItemTier = 1;
    [SerializeField, Min(1)] private int maxItemTier = 10;
    [SerializeField, Min(1)] private int requiredItemLevel = 1;

    [Header("Modifiers")]
    [SerializeField] private List<EquipmentModifierDefinition> modifiers = new List<EquipmentModifierDefinition>();

    public string AffixName => affixName;
    public EquipmentAffixType AffixType => affixType;
    public int AffixTier => affixTier;
    public string AffixTag => affixTag;
    public IReadOnlyList<EquipmentSlotType> AllowedSlots => allowedSlots;
    public int MinItemTier => minItemTier;
    public int MaxItemTier => maxItemTier;
    public int RequiredItemLevel => requiredItemLevel;
    public IReadOnlyList<EquipmentModifierDefinition> Modifiers => modifiers;

    public bool CanRollFor(EquipmentSlotType slotType, int itemTier, int itemLevel)
    {
        return itemTier >= minItemTier
            && itemTier <= maxItemTier
            && itemLevel >= requiredItemLevel
            && (allowedSlots == null || allowedSlots.Count == 0 || allowedSlots.Contains(slotType));
    }

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(affixName))
        {
            return false;
        }

        if (affixTier < 1 || maxItemTier < minItemTier || requiredItemLevel < 1 || modifiers == null || modifiers.Count == 0)
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
