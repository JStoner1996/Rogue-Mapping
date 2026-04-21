using System;
using UnityEngine;

[Serializable]
public struct AtlasNodeEffect
{
    public AtlasEffectType effectType;
    public float value;

    public bool IsConfigured()
    {
        return !Mathf.Approximately(value, 0f);
    }
}
