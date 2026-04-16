using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// The sparse decoration tiles painted on top of the ground layer.
[System.Serializable]
public class ChunkDecorationPalette
{
    [Header("Noise")]
    [SerializeField, Min(0.0001f)] private float noiseScale = 0.11f;
    [SerializeField, Range(0f, 1f)] private float placementThreshold = 0.82f;

    [Header("Tiles")]
    [SerializeField] private List<TileBase> decorationTiles = new List<TileBase>();

    public float NoiseScale => noiseScale;
    public float PlacementThreshold => placementThreshold;

    public bool HasTiles()
    {
        return decorationTiles.Count > 0;
    }

    public bool ShouldPlace(float noiseValue)
    {
        return noiseValue >= placementThreshold;
    }

    public TileBase GetTile(int variationIndex)
    {
        if (decorationTiles.Count == 0)
        {
            return null;
        }

        int tileIndex = Mathf.Abs(variationIndex) % decorationTiles.Count;
        return decorationTiles[tileIndex];
    }
}
