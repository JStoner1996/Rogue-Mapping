using System;
using UnityEngine;

public class ExpCrystal : MonoBehaviour, IItem
{
    public static event Action<int> onExpCrystalCollect;
    public int worth = 5;
    public void Collect()
    {
        onExpCrystalCollect.Invoke(worth);
        Destroy(gameObject);
    }

}
