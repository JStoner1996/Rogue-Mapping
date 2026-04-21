using System.Collections.Generic;

public class AtlasEffectSummary
{
    private readonly Dictionary<AtlasEffectType, float> valuesByEffect = new Dictionary<AtlasEffectType, float>();

    public void Add(AtlasNodeEffect effect)
    {
        if (!effect.IsConfigured())
        {
            return;
        }

        valuesByEffect.TryGetValue(effect.effectType, out float existingValue);
        valuesByEffect[effect.effectType] = existingValue + effect.value;
    }

    public float GetValue(AtlasEffectType effectType)
    {
        valuesByEffect.TryGetValue(effectType, out float value);
        return value;
    }
}
