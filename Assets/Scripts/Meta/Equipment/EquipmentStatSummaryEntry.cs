using System;

[Serializable]
public class EquipmentStatSummaryEntry
{
    public EquipmentStatType statType;
    public float flatValue;
    public float percentValue;

    public EquipmentStatSummaryEntry(EquipmentStatType statType)
    {
        this.statType = statType;
    }

    public bool HasAnyValue => !UnityEngine.Mathf.Approximately(flatValue, 0f) || !UnityEngine.Mathf.Approximately(percentValue, 0f);
    public bool HasFlatValue => !UnityEngine.Mathf.Approximately(flatValue, 0f);
    public bool HasPercentValue => !UnityEngine.Mathf.Approximately(percentValue, 0f);
}
