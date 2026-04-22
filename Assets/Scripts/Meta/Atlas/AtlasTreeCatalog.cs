using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AtlasTreeCatalog", menuName = "Atlas/Tree Catalog")]
public class AtlasTreeCatalog : ScriptableObject
{
    [SerializeField] private List<AtlasTreeDefinition> trees = new List<AtlasTreeDefinition>();

    public IReadOnlyList<AtlasTreeDefinition> Trees => trees;
}
