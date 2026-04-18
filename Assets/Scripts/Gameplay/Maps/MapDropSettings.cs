using System;
using UnityEngine;

[Serializable]
public class MapDropSettings
{
    [Header("Tier Weights")]
    [Min(0f)] public float sameTierWeight = 60f;
    [Min(0f)] public float aboveTierWeight = 30f;
    [Min(0f)] public float belowTierWeight = 10f;
}
