using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AtlasTreeUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private AtlasTreeDefinition treeDefinition;

    [Header("Components")]
    [SerializeField] private RectTransform treeRoot;
    [SerializeField] private RectTransform connectionRoot;
    [SerializeField] private RectTransform nodeRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button refundTreeButton;
    [SerializeField] private AtlasNodeDetailsPanelUI nodeDetailsPanel;
    [SerializeField] private AtlasNodeUI nodePrefab;
    [SerializeField] private AtlasConnectionUI connectionPrefab;

    private readonly Dictionary<string, AtlasNodeUI> nodeViewsById = new Dictionary<string, AtlasNodeUI>();
    private readonly List<AtlasConnectionUI> spawnedConnections = new List<AtlasConnectionUI>();
    private AtlasScreenUI screen;

    public AtlasTreeDefinition TreeDefinition => treeDefinition;

    void Awake()
    {
        RegisterButtons();
    }

    public void Initialize(AtlasScreenUI atlasScreen)
    {
        screen = atlasScreen;
        Build();
        Refresh();
    }

    public void Refresh()
    {
        if (treeDefinition == null)
        {
            return;
        }

        if (titleText != null)
        {
            titleText.text = treeDefinition.DisplayName;
        }

        foreach (AtlasNodeDefinition node in treeDefinition.Nodes)
        {
            if (node == null || !nodeViewsById.TryGetValue(node.NodeId, out AtlasNodeUI nodeView))
            {
                continue;
            }

            bool isAllocated = MetaProgressionService.IsAtlasNodeAllocated(treeDefinition.Category, node.NodeId);
            bool canAllocate = MetaProgressionService.CanAllocateAtlasNode(treeDefinition, node);
            bool canRefund = MetaProgressionService.CanRefundAtlasNode(treeDefinition, node);
            nodeView.SetState(isAllocated, canAllocate, canRefund);
        }

        if (refundTreeButton != null)
        {
            refundTreeButton.interactable = MetaProgressionService.GetAllocatedAtlasNodeIds(treeDefinition.Category).Count > 0;
        }
    }

    private void Build()
    {
        ClearSpawnedContent();

        if (treeDefinition == null || nodePrefab == null)
        {
            return;
        }

        foreach (AtlasNodeDefinition node in treeDefinition.Nodes)
        {
            if (node == null)
            {
                continue;
            }

            AtlasNodeUI nodeView = Instantiate(nodePrefab, nodeRoot != null ? nodeRoot : treeRoot);
            nodeView.Bind(node, HandleNodeClicked, HandleNodeHoverEnter, HandleNodeHoverExit);
            nodeViewsById[node.NodeId] = nodeView;
        }

        BuildConnections();
    }

    private void BuildConnections()
    {
        if (treeDefinition == null || connectionPrefab == null)
        {
            return;
        }

        foreach (AtlasNodeDefinition node in treeDefinition.Nodes)
        {
            if (node == null || !nodeViewsById.TryGetValue(node.NodeId, out AtlasNodeUI targetNodeView))
            {
                continue;
            }

            foreach (AtlasNodeDefinition prerequisite in node.PrerequisiteNodes)
            {
                if (prerequisite == null || !nodeViewsById.TryGetValue(prerequisite.NodeId, out AtlasNodeUI prerequisiteNodeView))
                {
                    continue;
                }

                AtlasConnectionUI connection = Instantiate(connectionPrefab, connectionRoot != null ? connectionRoot : treeRoot);
                connection.SetEndpoints(prerequisiteNodeView.Position, targetNodeView.Position);
                spawnedConnections.Add(connection);
            }
        }
    }

    private void HandleNodeClicked(AtlasNodeDefinition node)
    {
        if (treeDefinition == null || node == null)
        {
            return;
        }

        bool isAllocated = MetaProgressionService.IsAtlasNodeAllocated(treeDefinition.Category, node.NodeId);

        if (isAllocated)
        {
            if (!MetaProgressionService.TryRefundAtlasNode(treeDefinition, node))
            {
                SetFeedback(MetaProgressionService.GetAtlasRefundBlockReasonText(treeDefinition, node));
                return;
            }

            SetFeedback($"Refunded {node.DisplayName}.");
        }
        else
        {
            if (!MetaProgressionService.TryAllocateAtlasNode(treeDefinition, node))
            {
                SetFeedback(MetaProgressionService.GetAtlasAllocationBlockReasonText(treeDefinition, node));
                return;
            }

            SetFeedback($"Allocated {node.DisplayName}.");
        }

        screen?.Refresh();
    }

    private void HandleNodeHoverEnter(AtlasNodeDefinition node)
    {
        nodeDetailsPanel?.ShowNode(treeDefinition, node);
    }

    private void HandleNodeHoverExit()
    {
        nodeDetailsPanel?.Hide();
    }

    private void HandleRefundTreeClicked()
    {
        if (treeDefinition == null)
        {
            return;
        }

        int refundedCount = MetaProgressionService.RefundAtlasTree(treeDefinition);
        SetFeedback(refundedCount > 0
            ? $"Refunded {refundedCount} node{(refundedCount == 1 ? string.Empty : "s")}."
            : "No nodes to refund.");
        screen?.Refresh();
    }

    private void RegisterButtons()
    {
        if (refundTreeButton != null)
        {
            refundTreeButton.onClick.RemoveListener(HandleRefundTreeClicked);
            refundTreeButton.onClick.AddListener(HandleRefundTreeClicked);
        }
    }

    private void ClearSpawnedContent()
    {
        nodeViewsById.Clear();

        foreach (AtlasConnectionUI connection in spawnedConnections)
        {
            if (connection != null)
            {
                Destroy(connection.gameObject);
            }
        }

        spawnedConnections.Clear();

        RectTransform activeNodeRoot = nodeRoot != null ? nodeRoot : treeRoot;
        if (activeNodeRoot == null)
        {
            return;
        }

        for (int i = activeNodeRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(activeNodeRoot.GetChild(i).gameObject);
        }
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
    }
}
