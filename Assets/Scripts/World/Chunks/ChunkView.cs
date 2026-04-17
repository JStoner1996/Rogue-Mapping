using UnityEngine;
using UnityEngine.Tilemaps;

// Renders one generated chunk into tilemaps and spawned world objects.
public class ChunkView : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap decorationTilemap;

    [Header("Chunk Objects")]
    [SerializeField] private Transform objectsRoot;

    private ChunkCoordinate currentCoordinate;
    private ShrineObjective spawnedShrine;

    public void Render(
        ChunkData chunkData,
        ChunkTerrainPalette terrainPalette,
        ChunkDecorationPalette decorationPalette,
        int worldSeed,
        ShrineObjective shrinePrefab,
        int chunkSizeTiles)
    {
        currentCoordinate = chunkData.Coordinate;
        transform.position = chunkData.WorldOrigin;

        RenderGround(terrainPalette, worldSeed, chunkSizeTiles);
        RenderDecorations(decorationPalette, worldSeed, chunkSizeTiles);
        RenderShrine(chunkData, shrinePrefab);
    }

    // Terrain is sampled in world space so patterns continue across chunk borders.
    private void RenderGround(
        ChunkTerrainPalette terrainPalette,
        int worldSeed,
        int chunkSizeTiles)
    {
        if (groundTilemap == null)
        {
            return;
        }

        groundTilemap.ClearAllTiles();

        if (terrainPalette == null || !terrainPalette.HasAnyTiles() || chunkSizeTiles <= 0)
        {
            return;
        }

        TileBase[] tiles = new TileBase[chunkSizeTiles * chunkSizeTiles];
        int index = 0;

        for (int y = 0; y < chunkSizeTiles; y++)
        {
            for (int x = 0; x < chunkSizeTiles; x++)
            {
                int worldTileX = currentCoordinate.x * chunkSizeTiles + x;
                int worldTileY = currentCoordinate.y * chunkSizeTiles + y;
                float noiseValue = SampleTerrainNoise(worldTileX, worldTileY, worldSeed, terrainPalette.NoiseScale);
                int variationIndex = GetVariationIndex(worldTileX, worldTileY, worldSeed);
                tiles[index++] = terrainPalette.GetTileForNoise(noiseValue, variationIndex);
            }
        }

        groundTilemap.SetTilesBlock(
            new BoundsInt(0, 0, 0, chunkSizeTiles, chunkSizeTiles, 1),
            tiles);
    }

    private void RenderShrine(ChunkData chunkData, ShrineObjective shrinePrefab)
    {
        ClearShrine();

        if (!chunkData.HasShrine
            || chunkData.ShrineDefinition == null
            || shrinePrefab == null
            || ChunkRuntimeState.IsShrineConsumed(chunkData.Coordinate))
        {
            return;
        }

        Transform parent = objectsRoot != null ? objectsRoot : transform;
        spawnedShrine = Instantiate(shrinePrefab, parent);
        spawnedShrine.transform.localPosition = chunkData.ShrineLocalPosition;
        spawnedShrine.Configure(chunkData.ShrineDefinition);
        spawnedShrine.Activated += HandleShrineActivated;
    }

    private void HandleShrineActivated(ShrineObjective shrine)
    {
        ChunkRuntimeState.MarkShrineConsumed(currentCoordinate);
    }

    private void ClearShrine()
    {
        if (spawnedShrine == null)
        {
            return;
        }

        spawnedShrine.Activated -= HandleShrineActivated;
        Destroy(spawnedShrine.gameObject);
        spawnedShrine = null;
    }

    void OnDestroy()
    {
        if (spawnedShrine != null)
        {
            spawnedShrine.Activated -= HandleShrineActivated;
        }
    }

    // Decorations are intentionally sparse so the base terrain still reads clearly.
    private void RenderDecorations(ChunkDecorationPalette decorationPalette, int worldSeed, int chunkSizeTiles)
    {
        if (decorationTilemap == null)
        {
            return;
        }

        decorationTilemap.ClearAllTiles();

        if (decorationPalette == null || !decorationPalette.HasTiles() || chunkSizeTiles <= 0)
        {
            return;
        }

        TileBase[] tiles = new TileBase[chunkSizeTiles * chunkSizeTiles];
        int index = 0;

        for (int y = 0; y < chunkSizeTiles; y++)
        {
            for (int x = 0; x < chunkSizeTiles; x++)
            {
                int worldTileX = currentCoordinate.x * chunkSizeTiles + x;
                int worldTileY = currentCoordinate.y * chunkSizeTiles + y;
                float noiseValue = SampleDecorationNoise(worldTileX, worldTileY, worldSeed, decorationPalette.NoiseScale);
                int variationIndex = GetVariationIndex(worldTileX, worldTileY, worldSeed + 7919);
                tiles[index++] = decorationPalette.ShouldPlace(noiseValue)
                    ? decorationPalette.GetTile(variationIndex)
                    : null;
            }
        }

        decorationTilemap.SetTilesBlock(
            new BoundsInt(0, 0, 0, chunkSizeTiles, chunkSizeTiles, 1),
            tiles);
    }

    private float SampleTerrainNoise(int worldTileX, int worldTileY, int worldSeed, float noiseScale)
    {
        float sampleX = (worldTileX + worldSeed * 0.173f) * noiseScale;
        float sampleY = (worldTileY + worldSeed * 0.271f) * noiseScale;
        return Mathf.PerlinNoise(sampleX, sampleY);
    }

    private int GetVariationIndex(int worldTileX, int worldTileY, int worldSeed)
    {
        unchecked
        {
            int hash = worldSeed;
            hash = (hash * 397) ^ worldTileX;
            hash = (hash * 397) ^ worldTileY;
            return hash;
        }
    }

    private float SampleDecorationNoise(int worldTileX, int worldTileY, int worldSeed, float noiseScale)
    {
        float sampleX = (worldTileX + worldSeed * 0.347f) * noiseScale;
        float sampleY = (worldTileY + worldSeed * 0.419f) * noiseScale;
        return Mathf.PerlinNoise(sampleX, sampleY);
    }
}
