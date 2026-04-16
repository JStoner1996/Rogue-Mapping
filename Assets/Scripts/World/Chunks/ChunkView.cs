using UnityEngine;
using UnityEngine.Tilemaps;

// Renders one generated chunk into tilemaps and spawned world objects.
public class ChunkView : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;

    [Header("Chunk Objects")]
    [SerializeField] private Transform objectsRoot;

    private ChunkCoordinate currentCoordinate;
    private ShrineObjective spawnedShrine;

    public void Render(ChunkData chunkData, TileBase groundTile, ShrineObjective shrinePrefab, int chunkSizeTiles)
    {
        currentCoordinate = chunkData.Coordinate;
        transform.position = chunkData.WorldOrigin;

        RenderGround(groundTile, chunkSizeTiles);
        RenderShrine(chunkData, shrinePrefab);
    }

    private void RenderGround(TileBase groundTile, int chunkSizeTiles)
    {
        if (groundTilemap == null)
        {
            return;
        }

        groundTilemap.ClearAllTiles();

        if (groundTile == null || chunkSizeTiles <= 0)
        {
            return;
        }

        TileBase[] tiles = new TileBase[chunkSizeTiles * chunkSizeTiles];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = groundTile;
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
}
