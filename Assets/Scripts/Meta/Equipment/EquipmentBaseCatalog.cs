using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentBaseCatalog", menuName = "Equipment/Base Catalog")]
public class EquipmentBaseCatalog : ScriptableObject
{
    [SerializeField] private List<EquipmentBaseDefinition> baseDefinitions = new List<EquipmentBaseDefinition>();

    public IReadOnlyList<EquipmentBaseDefinition> BaseDefinitions => baseDefinitions;

    public List<EquipmentBaseDefinition> GetValidBases(EquipmentSlotType slotType, int itemTier)
    {
        List<EquipmentBaseDefinition> validBases = new List<EquipmentBaseDefinition>();

        foreach (EquipmentBaseDefinition baseDefinition in baseDefinitions)
        {
            if (baseDefinition == null || !baseDefinition.IsConfigured())
            {
                continue;
            }

            if (baseDefinition.SlotType != slotType || !baseDefinition.CanRollAtTier(itemTier))
            {
                continue;
            }

            validBases.Add(baseDefinition);
        }

        return validBases;
    }
}
