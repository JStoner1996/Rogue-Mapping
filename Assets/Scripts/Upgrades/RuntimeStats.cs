using UnityEngine;

[System.Serializable]
public class RuntimeStats
{
    public float baseDamage;
    public float damageMultiplier = 0f;
    public float globalDamageMultiplier = 0f;
    public float Damage => baseDamage * (1f + damageMultiplier + globalDamageMultiplier);

    public float baseAttackSpeed = 1f;
    public float attackSpeedMultiplier = 0f;
    public float globalAttackSpeedMultiplier = 0f;
    public float AttackSpeed => baseAttackSpeed * (1f + attackSpeedMultiplier + globalAttackSpeedMultiplier);

    public float baseRange;
    public float rangeMultiplier = 0f;
    public float globalRangeMultiplier = 0f;
    public float Range => baseRange * (1f + rangeMultiplier + globalRangeMultiplier);

    public float baseKnockback;
    public float knockbackMultiplier = 0f;
    public float globalKnockbackMultiplier = 0f;
    public float Knockback => baseKnockback * (1f + knockbackMultiplier + globalKnockbackMultiplier);

    public float duration;
    public float cooldown;
    public float projectileSpeed;
    public int bounceCount;

}
