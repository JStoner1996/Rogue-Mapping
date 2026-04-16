using UnityEngine;

// Shared helpers for translating between world space and chunk space.
public static class ChunkWorldUtility
{
    public static float GetChunkWorldSize(int chunkSizeTiles, float tileSize)
    {
        return chunkSizeTiles * tileSize;
    }

    public static ChunkCoordinate GetChunkCoordinate(Vector3 worldPosition, int chunkSizeTiles, float tileSize)
    {
        float chunkWorldSize = GetChunkWorldSize(chunkSizeTiles, tileSize);
        return new ChunkCoordinate(
            Mathf.FloorToInt(worldPosition.x / chunkWorldSize),
            Mathf.FloorToInt(worldPosition.y / chunkWorldSize));
    }

    public static Vector3 GetChunkWorldOrigin(ChunkCoordinate coordinate, int chunkSizeTiles, float tileSize)
    {
        float chunkWorldSize = GetChunkWorldSize(chunkSizeTiles, tileSize);
        return new Vector3(
            coordinate.x * chunkWorldSize,
            coordinate.y * chunkWorldSize,
            0f);
    }

    public static Vector3 GetChunkWorldCenter(ChunkCoordinate coordinate, int chunkSizeTiles, float tileSize)
    {
        float halfChunkWorldSize = GetChunkWorldSize(chunkSizeTiles, tileSize) * 0.5f;
        Vector3 origin = GetChunkWorldOrigin(coordinate, chunkSizeTiles, tileSize);
        return new Vector3(
            origin.x + halfChunkWorldSize,
            origin.y + halfChunkWorldSize,
            origin.z);
    }

    public static int GetChebyshevDistance(ChunkCoordinate a, ChunkCoordinate b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }

    public static int GetManhattanDistance(ChunkCoordinate a, ChunkCoordinate b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
