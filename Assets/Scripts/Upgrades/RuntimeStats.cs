using UnityEngine;

[System.Serializable]
public class RuntimeStats
{
    public float damage;
    public float baseAttackSpeed = 1f;
    public float attackSpeedMultiplier = 2f;
    public float AttackSpeed => baseAttackSpeed * (1f + attackSpeedMultiplier);
    public float baseRange;
    public float rangeMultiplier = 0f;
    public float Range => baseRange * (1f + rangeMultiplier);
    public float duration;
    public float cooldown;
    public int bounceCount;

}