using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapCatalog", menuName = "Maps/Map Catalog")]
public class MapCatalog : ScriptableObject
{
    [SerializeField] private List<MapBaseDefinition> baseMaps = new List<MapBaseDefinition>();

    public IReadOnlyList<MapBaseDefinition> BaseMaps => baseMaps;
}
