public readonly struct DamageRoll
{
    public DamageRoll(float damage, bool isCritical, float criticalDamageMultiplier)
    {
        Damage = damage;
        IsCritical = isCritical;
        CriticalDamageMultiplier = criticalDamageMultiplier;
    }

    public readonly float Damage;
    public readonly bool IsCritical;
    public readonly float CriticalDamageMultiplier;
}
