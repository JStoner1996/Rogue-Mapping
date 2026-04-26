using System;

[Serializable]
public class PlayerStatRoll
{
    public PlayerStatType statType;
    public bool usesFlatValue;
    public float minValue;
    public float maxValue;
    public int weight;
}
