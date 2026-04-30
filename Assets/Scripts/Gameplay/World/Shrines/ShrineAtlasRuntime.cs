using UnityEngine;

public static class ShrineAtlasRuntime
{
    private static float activeShrineBuffUntil;

    public static bool HasActiveShrineBuff => Time.time < activeShrineBuffUntil;

    public static float GetPercentMultiplier(AtlasEffectType effectType)
    {
        return Mathf.Max(0f, 1f + MetaProgressionService.GetAtlasEffectValue(effectType) / 100f);
    }

    public static int GetFlatCount(AtlasEffectType effectType)
    {
        return Mathf.Max(0, Mathf.RoundToInt(MetaProgressionService.GetAtlasEffectValue(effectType)));
    }

    public static void RegisterActiveShrineBuff(float durationSeconds)
    {
        if (durationSeconds > 0f)
        {
            activeShrineBuffUntil = Mathf.Max(activeShrineBuffUntil, Time.time + durationSeconds);
        }
    }

    public static void TrySpawnShrineFromEnemyKill(Vector3 position)
    {
        if (!HasActiveShrineBuff || !RollAdditiveChance(AtlasEffectType.ShrineBuffKillSpawnChancePercent))
        {
            return;
        }

        WorldChunkManager chunkManager = WorldChunkManager.Instance ?? Object.FindAnyObjectByType<WorldChunkManager>();
        chunkManager?.TrySpawnShrineAt(position);
    }

    private static bool RollAdditiveChance(AtlasEffectType effectType)
    {
        float chance = MetaProgressionService.GetAtlasEffectValue(effectType) / 100f;
        return chance > 0f && Random.value <= Mathf.Clamp01(chance);
    }
}
