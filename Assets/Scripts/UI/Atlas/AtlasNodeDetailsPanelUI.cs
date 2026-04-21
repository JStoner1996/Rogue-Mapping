using TMPro;
using UnityEngine;

public class AtlasNodeDetailsPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private HoverPopupFollowerUI hoverFollower;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Color normalStatusColor = Color.white;
    [SerializeField] private Color blockedStatusColor = new Color(1f, 0.35f, 0.35f, 1f);

    public void ShowNode(AtlasTreeDefinition tree, AtlasNodeDefinition node)
    {
        if (tree == null || node == null)
        {
            Hide();
            return;
        }

        SetVisible(true);

        if (titleText != null)
        {
            titleText.text = node.DisplayName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = node.Description;
        }

        if (statusText != null)
        {
            bool isAllocated = MetaProgressionService.IsAtlasNodeAllocated(tree.Category, node.NodeId);
            string statusMessage = isAllocated
                ? MetaProgressionService.GetAtlasRefundBlockReasonText(tree, node)
                : MetaProgressionService.GetAtlasAllocationBlockReasonText(tree, node);

            statusText.text = string.IsNullOrWhiteSpace(statusMessage) ? string.Empty : statusMessage;
            statusText.color = string.IsNullOrWhiteSpace(statusMessage) ? normalStatusColor : blockedStatusColor;
            statusText.gameObject.SetActive(!string.IsNullOrWhiteSpace(statusMessage));
        }
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool isVisible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(isVisible);
        }

        if (hoverFollower != null)
        {
            if (isVisible)
            {
                hoverFollower.BeginFollowing();
            }
            else
            {
                hoverFollower.StopFollowing();
            }
        }
    }
}
