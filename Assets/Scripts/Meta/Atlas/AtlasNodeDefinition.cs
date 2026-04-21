using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AtlasNode", menuName = "Atlas/Node Definition")]
public class AtlasNodeDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string nodeId;
    [SerializeField] private string displayName;
    [SerializeField, TextArea(2, 4)] private string description;
    [SerializeField] private AtlasCategoryType category;
    [SerializeField] private AtlasNodeSize nodeSize;
    [SerializeField] private bool isRootNode;

    [Header("Layout")]
    [SerializeField] private Vector2 editorPosition;

    [Header("Graph")]
    // A node is allocatable when at least one allocated prerequisite leads into it.
    [SerializeField] private List<AtlasNodeDefinition> prerequisiteNodes = new List<AtlasNodeDefinition>();

    [Header("Effects")]
    [SerializeField] private List<AtlasNodeEffect> effects = new List<AtlasNodeEffect>();

    public string NodeId => nodeId;
    public string DisplayName => displayName;
    public string Description => description;
    public AtlasCategoryType Category => category;
    public AtlasNodeSize NodeSize => nodeSize;
    public bool IsRootNode => isRootNode;
    public Vector2 EditorPosition => editorPosition;
    public IReadOnlyList<AtlasNodeDefinition> PrerequisiteNodes => prerequisiteNodes;
    public IReadOnlyList<AtlasNodeEffect> Effects => effects;

    public bool IsConfigured()
    {
        if (string.IsNullOrWhiteSpace(nodeId) || string.IsNullOrWhiteSpace(displayName))
        {
            return false;
        }

        if (isRootNode)
        {
            return true;
        }

        return prerequisiteNodes != null && prerequisiteNodes.Count > 0;
    }
}
