using System.Collections.Generic;
using UnityEngine;

// The per-map world presentation data used by the shared Game scene.
[CreateAssetMenu(fileName = "MapWorldTheme", menuName = "Maps/Map World Theme")]
public class MapWorldThemeDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string themeName = "New Theme";

    [Header("Chunk Presentation")]
    [SerializeField] private ChunkTerrainPalette terrainPalette = new ChunkTerrainPalette();
    [SerializeField] private ChunkDecorationPalette decorationPalette = new ChunkDecorationPalette();

    [Header("Chunk Objectives")]
    [SerializeField] private ShrineObjective shrinePrefab;
    [SerializeField] private List<ShrineDefinition> shrineDefinitions = new List<ShrineDefinition>();

    public string ThemeName => string.IsNullOrWhiteSpace(themeName) ? name : themeName;
    public ChunkTerrainPalette TerrainPalette => terrainPalette;
    public ChunkDecorationPalette DecorationPalette => decorationPalette;
    public ShrineObjective ShrinePrefab => shrinePrefab;
    public IReadOnlyList<ShrineDefinition> ShrineDefinitions => shrineDefinitions;

    public bool HasTerrainPalette()
    {
        return terrainPalette != null && terrainPalette.HasAnyTiles();
    }

    public bool HasDecorationPalette()
    {
        return decorationPalette != null && decorationPalette.HasTiles();
    }

    public bool HasShrineContent()
    {
        return shrinePrefab != null && shrineDefinitions.Count > 0;
    }
}
