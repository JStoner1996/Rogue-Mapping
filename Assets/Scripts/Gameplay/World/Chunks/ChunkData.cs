using UnityEngine;

// The generated data that describes one chunk before it is rendered.
public sealed class ChunkData
{
    public ChunkCoordinate Coordinate { get; }
    public Vector3 WorldOrigin { get; }
    public bool HasShrine { get; }
    public Vector3 ShrineLocalPosition { get; }
    public ShrineDefinition ShrineDefinition { get; }

    public ChunkData(
        ChunkCoordinate coordinate,
        Vector3 worldOrigin,
        bool hasShrine,
        Vector3 shrineLocalPosition,
        ShrineDefinition shrineDefinition)
    {
        Coordinate = coordinate;
        WorldOrigin = worldOrigin;
        HasShrine = hasShrine;
        ShrineLocalPosition = shrineLocalPosition;
        ShrineDefinition = shrineDefinition;
    }
}
