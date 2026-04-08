using System;
using UnityEngine;

public class HealthPickup : MonoBehaviour, IItem
{

    public static event Action<int> onHealthPickup;
    public int worth = 5;
    public void Collect()
    {
        onHealthPickup?.Invoke(worth);
        PickupPools.Instance.ReturnHealth(this);
        AudioManager.Instance.Play(SoundType.Heal);

    }


}
