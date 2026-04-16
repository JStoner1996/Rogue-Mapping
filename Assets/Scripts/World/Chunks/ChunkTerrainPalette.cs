using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// The editor-facing terrain bands used to paint chunk ground tiles from noise.
[System.Serializable]
public class ChunkTerrainPalette
{
    [Header("Noise")]
    [SerializeField, Min(0.0001f)] private float noiseScale = 0.05f;
    [SerializeField, Range(0f, 1f)] private float lowBandThreshold = 0.35f;
    [SerializeField, Range(0f, 1f)] private float highBandThreshold = 0.65f;

    [Header("Terrain Bands")]
    [SerializeField] private List<TileBase> lowBandTiles = new List<TileBase>();
    [SerializeField] private List<TileBase> midBandTiles = new List<TileBase>();
    [SerializeField] private List<TileBase> highBandTiles = new List<TileBase>();

    public float NoiseScale => noiseScale;
    public float LowBandThreshold => lowBandThreshold;
    public float HighBandThreshold => highBandThreshold;

    public TileBase GetTileForNoise(float noiseValue, int variationIndex)
    {
        IReadOnlyList<TileBase> sourceTiles = GetBandTiles(noiseValue);
        if (sourceTiles == null || sourceTiles.Count == 0)
        {
            return null;
        }

        int tileIndex = Mathf.Abs(variationIndex) % sourceTiles.Count;
        return sourceTiles[tileIndex];
    }

    public bool HasAnyTiles()
    {
        return lowBandTiles.Count > 0 || midBandTiles.Count > 0 || highBandTiles.Count > 0;
    }

    private IReadOnlyList<TileBase> GetBandTiles(float noiseValue)
    {
        if (noiseValue <= lowBandThreshold)
        {
            return lowBandTiles.Count > 0 ? lowBandTiles : GetFallbackTiles();
        }

        if (noiseValue >= highBandThreshold)
        {
            return highBandTiles.Count > 0 ? highBandTiles : GetFallbackTiles();
        }

        return midBandTiles.Count > 0 ? midBandTiles : GetFallbackTiles();
    }

    private IReadOnlyList<TileBase> GetFallbackTiles()
    {
        if (midBandTiles.Count > 0)
        {
            return midBandTiles;
        }

        if (lowBandTiles.Count > 0)
        {
            return lowBandTiles;
        }

        return highBandTiles;
    }
}
