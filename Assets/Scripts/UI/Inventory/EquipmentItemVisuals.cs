using UnityEngine;

// Keeps the shared equipment tint ramp in one place so inventory and loadout views stay visually aligned.
public static class EquipmentItemVisuals
{
    public static Color GetTierTint(int itemTier)
    {
        float normalizedTier = Mathf.InverseLerp(1f, 10f, Mathf.Clamp(itemTier, 1, 10));
        return Color.Lerp(Color.white, Color.red, normalizedTier);
    }
}
