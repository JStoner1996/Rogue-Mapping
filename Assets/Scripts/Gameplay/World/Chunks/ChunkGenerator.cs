using System;
using System.Collections.Generic;
using UnityEngine;

// Builds deterministic chunk data from a world seed and chunk coordinate.
public sealed class ChunkGenerator
{
    private readonly int worldSeed;
    private readonly int chunkSizeTiles;
    private readonly float tileSize;
    private readonly float shrineSpawnChance;
    private readonly float greaterShrineChance;
    private readonly int shrineEdgePaddingTiles;
    private readonly IReadOnlyList<ShrineDefinition> shrineDefinitions;

    public ChunkGenerator(
        int worldSeed,
        int chunkSizeTiles,
        float tileSize,
        float shrineSpawnChance,
        float greaterShrineChance,
        int shrineEdgePaddingTiles,
        IReadOnlyList<ShrineDefinition> shrineDefinitions)
    {
        this.worldSeed = worldSeed;
        this.chunkSizeTiles = chunkSizeTiles;
        this.tileSize = tileSize;
        this.shrineSpawnChance = shrineSpawnChance;
        this.greaterShrineChance = greaterShrineChance;
        this.shrineEdgePaddingTiles = shrineEdgePaddingTiles;
        this.shrineDefinitions = shrineDefinitions;
    }

    public ChunkData Generate(ChunkCoordinate coordinate)
    {
        System.Random random = new System.Random(GetChunkSeed(coordinate));
        Vector3 worldOrigin = ChunkWorldUtility.GetChunkWorldOrigin(coordinate, chunkSizeTiles, tileSize);

        bool hasShrine = ShouldSpawnShrine(random);
        ShrineDefinition shrineDefinition = hasShrine ? RollShrineDefinition(random) : null;
        bool isGreaterShrine = hasShrine && random.NextDouble() <= greaterShrineChance;
        Vector3 shrineLocalPosition = hasShrine
            ? RollShrineLocalPosition(random)
            : Vector3.zero;

        return new ChunkData(
            coordinate,
            worldOrigin,
            hasShrine,
            shrineLocalPosition,
            shrineDefinition,
            isGreaterShrine);
    }

    private bool ShouldSpawnShrine(System.Random random)
    {
        return shrineDefinitions != null
            && shrineDefinitions.Count > 0
            && random.NextDouble() <= shrineSpawnChance;
    }

    private ShrineDefinition RollShrineDefinition(System.Random random)
    {
        int index = random.Next(0, shrineDefinitions.Count);
        return shrineDefinitions[index];
    }

    // Shrines stay away from chunk edges so they are less awkward to encounter.
    private Vector3 RollShrineLocalPosition(System.Random random)
    {
        int minimumTile = Mathf.Clamp(shrineEdgePaddingTiles, 0, chunkSizeTiles - 1);
        int maximumTile = Mathf.Clamp(chunkSizeTiles - shrineEdgePaddingTiles - 1, minimumTile, chunkSizeTiles - 1);
        int tileX = random.Next(minimumTile, maximumTile + 1);
        int tileY = random.Next(minimumTile, maximumTile + 1);

        return new Vector3(
            (tileX + 0.5f) * tileSize,
            (tileY + 0.5f) * tileSize,
            0f);
    }

    private int GetChunkSeed(ChunkCoordinate coordinate)
    {
        unchecked
        {
            int hash = worldSeed;
            hash = (hash * 397) ^ coordinate.x;
            hash = (hash * 397) ^ coordinate.y;
            return hash;
        }
    }
}
