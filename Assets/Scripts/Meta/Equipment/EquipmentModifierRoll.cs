using System;

[Serializable]
public struct EquipmentModifierRoll
{
    public EquipmentStatType statType;
    public EquipmentModifierKind modifierKind;
    public float value;

    public EquipmentModifierRoll(EquipmentStatType statType, EquipmentModifierKind modifierKind, float value)
    {
        this.statType = statType;
        this.modifierKind = modifierKind;
        this.value = value;
    }
}
