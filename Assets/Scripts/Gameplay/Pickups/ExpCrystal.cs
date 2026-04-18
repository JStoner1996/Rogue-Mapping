using System;
using UnityEngine;

public class ExpCrystal : MonoBehaviour, IItem
{
    public static event Action<int> onExpCrystalCollect;
    public int worth = 5;

    public void Init(int value)
    {
        worth = value;
    }

    public void Collect()
    {
        onExpCrystalCollect?.Invoke(GetAdjustedWorth());
        PickupPools.Instance.ReturnXP(this);
        AudioManager.Instance.Play(SoundType.GetExp);
    }

    private int GetAdjustedWorth()
    {
        EquipmentStatSummaryEntry entry = MetaProgressionService.GetEquippedEquipmentStatSummary()?.GetEntry(EquipmentStatType.ExperienceGain);
        float multiplier = entry != null && entry.HasPercentValue
            ? Mathf.Max(0f, 1f + entry.percentValue)
            : 1f;

        return Mathf.Max(1, Mathf.RoundToInt(worth * multiplier));
    }

}
