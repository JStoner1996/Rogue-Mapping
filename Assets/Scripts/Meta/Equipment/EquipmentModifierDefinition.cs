using System;
using UnityEngine;

[Serializable]
public struct EquipmentModifierDefinition
{
    public EquipmentStatType statType;
    public EquipmentModifierKind modifierKind;
    public float minValue;
    public float maxValue;
    public EquipmentTierScalingMode tierScalingMode;
    public float tierScalingAmount;

    public bool UsesRange => !Mathf.Approximately(minValue, maxValue);

    public bool IsValid()
    {
        return maxValue >= minValue;
    }
}
