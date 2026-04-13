using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentAffixCatalog", menuName = "Equipment/Affix Catalog")]
public class EquipmentAffixCatalog : ScriptableObject
{
    [SerializeField] private List<EquipmentAffixDefinition> affixDefinitions = new List<EquipmentAffixDefinition>();

    public IReadOnlyList<EquipmentAffixDefinition> AffixDefinitions => affixDefinitions;

    public List<EquipmentAffixDefinition> GetValidAffixes(
        EquipmentAffixType affixType,
        EquipmentSlotType slotType,
        int itemTier)
    {
        List<EquipmentAffixDefinition> validAffixes = new List<EquipmentAffixDefinition>();

        foreach (EquipmentAffixDefinition affixDefinition in affixDefinitions)
        {
            if (affixDefinition == null || !affixDefinition.IsConfigured())
            {
                continue;
            }

            if (affixDefinition.AffixType != affixType || !affixDefinition.CanRollFor(slotType, itemTier))
            {
                continue;
            }

            validAffixes.Add(affixDefinition);
        }

        return validAffixes;
    }
}
