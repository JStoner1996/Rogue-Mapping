using System.Collections.Generic;

// The runtime-only state for generated chunks, like used shrine tracking.
public static class ChunkRuntimeState
{
    private static readonly HashSet<ChunkCoordinate> ConsumedShrineChunks = new HashSet<ChunkCoordinate>();

    public static void Reset()
    {
        ConsumedShrineChunks.Clear();
    }

    public static bool IsShrineConsumed(ChunkCoordinate coordinate)
    {
        return ConsumedShrineChunks.Contains(coordinate);
    }

    public static void MarkShrineConsumed(ChunkCoordinate coordinate)
    {
        ConsumedShrineChunks.Add(coordinate);
    }
}
