using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Loads and unloads generated chunks around the player.
public class WorldChunkManager : SingletonBehaviour<WorldChunkManager>
{
    [Header("Scene References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform chunkRoot;
    [SerializeField] private ChunkView chunkPrefab;

    [Header("Generation")]
    [SerializeField, Min(1)] private int chunkSizeTiles = 32;
    [SerializeField, Min(0.01f)] private float tileSize = 1f;
    [SerializeField] private int worldSeed = 12345;

    [Header("Scene Fallback Theme")]
    [FormerlySerializedAs("terrainPalette")]
    [SerializeField] private ChunkTerrainPalette fallbackTerrainPalette = new ChunkTerrainPalette();
    [FormerlySerializedAs("decorationPalette")]
    [SerializeField] private ChunkDecorationPalette fallbackDecorationPalette = new ChunkDecorationPalette();
    [FormerlySerializedAs("shrinePrefab")]
    [SerializeField] private ShrineObjective fallbackShrinePrefab;
    [FormerlySerializedAs("shrineDefinitions")]
    [SerializeField] private List<ShrineDefinition> fallbackShrineDefinitions = new List<ShrineDefinition>();

    [Header("Loading")]
    [SerializeField, Min(0)] private int loadRadius = 1;

    [Header("Enemy Cleanup")]
    [SerializeField, Min(0)] private int ambientEnemyDespawnChunkDistance = 3;

    [Header("Chunk Content")]
    [SerializeField, Range(0f, 1f)] private float shrineSpawnChance = 0.5f;
    [SerializeField, Min(0)] private int shrineEdgePaddingTiles = 4;

    private readonly Dictionary<ChunkCoordinate, ChunkView> activeChunks = new Dictionary<ChunkCoordinate, ChunkView>();
    private ChunkGenerator chunkGenerator;
    private ChunkCoordinate? currentPlayerChunk;
    private MapWorldThemeDefinition activeTheme;

    public int ChunkSizeTiles => chunkSizeTiles;
    public float TileSize => tileSize;
    public int AmbientEnemyDespawnChunkDistance => ambientEnemyDespawnChunkDistance;
    public int LoadRadius => loadRadius;
    public MapWorldThemeDefinition ActiveTheme => activeTheme;

    private void Awake()
    {
        if (!TryInitializeSingleton())
        {
            return;
        }

        ChunkRuntimeState.Reset();
        ResolveActiveTheme();
        chunkGenerator = BuildChunkGenerator();
    }

    private void Start()
    {
        RefreshLoadedChunks(true);
    }

    private void Update()
    {
        RefreshLoadedChunks(false);
    }

    private void RefreshLoadedChunks(bool force)
    {
        if (!TryGetPlayerTransform(out Transform player))
        {
            return;
        }

        ChunkCoordinate playerChunk = ChunkWorldUtility.GetChunkCoordinate(player.position, chunkSizeTiles, tileSize);
        if (!force && currentPlayerChunk.HasValue && currentPlayerChunk.Value.Equals(playerChunk))
        {
            return;
        }

        currentPlayerChunk = playerChunk;

        HashSet<ChunkCoordinate> requiredChunks = BuildRequiredChunkSet(playerChunk);
        UnloadDistantChunks(requiredChunks);
        LoadMissingChunks(requiredChunks);
    }

    private HashSet<ChunkCoordinate> BuildRequiredChunkSet(ChunkCoordinate center)
    {
        HashSet<ChunkCoordinate> required = new HashSet<ChunkCoordinate>();

        for (int y = -loadRadius; y <= loadRadius; y++)
        {
            for (int x = -loadRadius; x <= loadRadius; x++)
            {
                required.Add(new ChunkCoordinate(center.x + x, center.y + y));
            }
        }

        return required;
    }

    private void UnloadDistantChunks(HashSet<ChunkCoordinate> requiredChunks)
    {
        List<ChunkCoordinate> chunksToUnload = new List<ChunkCoordinate>();

        foreach (KeyValuePair<ChunkCoordinate, ChunkView> pair in activeChunks)
        {
            if (!requiredChunks.Contains(pair.Key))
            {
                chunksToUnload.Add(pair.Key);
            }
        }

        for (int i = 0; i < chunksToUnload.Count; i++)
        {
            ChunkCoordinate coordinate = chunksToUnload[i];
            if (activeChunks.TryGetValue(coordinate, out ChunkView chunkView))
            {
                Destroy(chunkView.gameObject);
            }

            activeChunks.Remove(coordinate);
        }
    }

    private void LoadMissingChunks(HashSet<ChunkCoordinate> requiredChunks)
    {
        foreach (ChunkCoordinate coordinate in requiredChunks)
        {
            if (activeChunks.ContainsKey(coordinate))
            {
                continue;
            }

            SpawnChunk(coordinate);
        }
    }

    private void SpawnChunk(ChunkCoordinate coordinate)
    {
        if (chunkPrefab == null)
        {
            return;
        }

        ChunkView chunkView = Instantiate(chunkPrefab, chunkRoot != null ? chunkRoot : transform);
        chunkView.name = $"Chunk_{coordinate.x}_{coordinate.y}";
        chunkView.Render(
            chunkGenerator.Generate(coordinate),
            GetThemeValue(theme => theme.HasTerrainPalette(), theme => theme.TerrainPalette, fallbackTerrainPalette),
            GetThemeValue(theme => theme.HasDecorationPalette(), theme => theme.DecorationPalette, fallbackDecorationPalette),
            worldSeed,
            GetThemeValue(theme => theme.HasShrineContent(), theme => theme.ShrinePrefab, fallbackShrinePrefab),
            chunkSizeTiles);
        activeChunks.Add(coordinate, chunkView);
    }

    private void ResolveActiveTheme()
    {
        activeTheme = RunData.GetSelectedMapOrDefault()?.WorldTheme;
    }

    private ChunkGenerator BuildChunkGenerator()
    {
        return new ChunkGenerator(
            worldSeed,
            chunkSizeTiles,
            tileSize,
            GetEffectiveShrineSpawnChance(),
            shrineEdgePaddingTiles,
            GetShrineDefinitions());
    }

    // The selected map theme wins. The scene fallback keeps the shared Game scene safe while themes are being authored.
    private IReadOnlyList<ShrineDefinition> GetShrineDefinitions() =>
        GetThemeValue(theme => theme.HasShrineContent(), theme => theme.ShrineDefinitions, fallbackShrineDefinitions);

    private float GetEffectiveShrineSpawnChance()
    {
        EquipmentStatSummary summary = MetaProgressionService.GetEquippedEquipmentStatSummary();
        float quantityModifier = summary?.GetEntry(EquipmentStatType.ShrineQuantity)?.percentValue ?? 0f;
        return Mathf.Clamp01(shrineSpawnChance * Mathf.Max(0f, 1f + quantityModifier));
    }

    private bool TryGetPlayerTransform(out Transform player)
    {
        if (playerTransform == null && PlayerController.Instance != null)
        {
            playerTransform = PlayerController.Instance.transform;
        }

        player = playerTransform;
        return player != null;
    }

    public bool TryGetPlayerChunk(out ChunkCoordinate playerChunk)
    {
        if (!TryGetPlayerTransform(out Transform player))
        {
            playerChunk = default;
            return false;
        }

        playerChunk = ChunkWorldUtility.GetChunkCoordinate(player.position, chunkSizeTiles, tileSize);
        return true;
    }

    public bool IsChunkLoaded(ChunkCoordinate coordinate) => activeChunks.ContainsKey(coordinate);

    public Vector2 GetRandomPointInChunk(ChunkCoordinate coordinate, float edgePaddingWorld = 0f)
    {
        Vector3 origin = ChunkWorldUtility.GetChunkWorldOrigin(coordinate, chunkSizeTiles, tileSize);
        float chunkWorldSize = ChunkWorldUtility.GetChunkWorldSize(chunkSizeTiles, tileSize);
        float padding = Mathf.Clamp(edgePaddingWorld, 0f, chunkWorldSize * 0.5f - 0.01f);

        return new Vector2(
            Random.Range(origin.x + padding, origin.x + chunkWorldSize - padding),
            Random.Range(origin.y + padding, origin.y + chunkWorldSize - padding));
    }

    private T GetThemeValue<T>(
        System.Func<MapWorldThemeDefinition, bool> hasValue,
        System.Func<MapWorldThemeDefinition, T> selectValue,
        T fallback)
    {
        return activeTheme != null && hasValue(activeTheme) ? selectValue(activeTheme) : fallback;
    }
}
