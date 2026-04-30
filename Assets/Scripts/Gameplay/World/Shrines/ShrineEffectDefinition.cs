using UnityEngine;

// The reusable behavior a shrine runs once it fully charges.
public abstract class ShrineEffectDefinition : ScriptableObject
{
    public abstract void Activate(ShrineObjective shrine);

    public virtual void Activate(ShrineObjective shrine, float effectMultiplier, float durationMultiplier)
    {
        Activate(shrine);
    }
}
