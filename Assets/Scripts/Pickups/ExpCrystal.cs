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
        onExpCrystalCollect?.Invoke(worth);
        PickupPools.Instance.ReturnXP(this);
        AudioManager.Instance.Play(SoundType.GetExp);
    }

}
