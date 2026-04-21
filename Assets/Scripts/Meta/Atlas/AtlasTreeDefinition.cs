using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AtlasTree", menuName = "Atlas/Tree Definition")]
public class AtlasTreeDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private AtlasCategoryType category;
    [SerializeField] private string displayName;
    [SerializeField, TextArea(2, 4)] private string description;

    [Header("Nodes")]
    [SerializeField] private List<AtlasNodeDefinition> nodes = new List<AtlasNodeDefinition>();

    public AtlasCategoryType Category => category;
    public string DisplayName => displayName;
    public string Description => description;
    public IReadOnlyList<AtlasNodeDefinition> Nodes => nodes;

    public AtlasNodeDefinition FindNodeById(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return null;
        }

        return nodes.Find(node => node != null && node.NodeId == nodeId);
    }

    public List<AtlasNodeDefinition> GetRootNodes()
    {
        return nodes.FindAll(node => node != null && node.IsRootNode);
    }

    public bool ContainsNode(AtlasNodeDefinition node)
    {
        return node != null && nodes.Contains(node);
    }
}
