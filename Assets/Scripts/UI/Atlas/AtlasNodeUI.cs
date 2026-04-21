using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AtlasNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Components")]
    [SerializeField] private Button button;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private bool showNameOnNode;

    [Header("Sizing")]
    [SerializeField] private Vector2 smallNodeSize = new Vector2(64f, 64f);
    [SerializeField] private Vector2 bigNodeSize = new Vector2(96f, 96f);

    [Header("State Colors")]
    [SerializeField] private Color lockedColor = new Color(0.28f, 0.28f, 0.28f, 1f);
    [SerializeField] private Color availableColor = new Color(0.72f, 0.60f, 0.28f, 1f);
    [SerializeField] private Color allocatedColor = new Color(0.95f, 0.83f, 0.38f, 1f);
    [SerializeField] private Color refundColor = new Color(0.45f, 0.82f, 0.95f, 1f);
    [SerializeField] private Color disabledTextColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    [SerializeField] private Color enabledTextColor = Color.white;

    private AtlasNodeDefinition boundNode;
    private System.Action<AtlasNodeDefinition> clickHandler;
    private System.Action<AtlasNodeDefinition> hoverEnterHandler;
    private System.Action hoverExitHandler;

    public AtlasNodeDefinition BoundNode => boundNode;
    public Vector2 Position => rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;

    public void Bind(
        AtlasNodeDefinition node,
        System.Action<AtlasNodeDefinition> onClicked,
        System.Action<AtlasNodeDefinition> onHoverEnter,
        System.Action onHoverExit)
    {
        boundNode = node;
        clickHandler = onClicked;
        hoverEnterHandler = onHoverEnter;
        hoverExitHandler = onHoverExit;

        if (nameText != null)
        {
            nameText.text = showNameOnNode && node != null ? node.DisplayName : string.Empty;
            nameText.gameObject.SetActive(showNameOnNode);
        }

        if (rectTransform != null && node != null)
        {
            rectTransform.anchoredPosition = node.EditorPosition;
            rectTransform.sizeDelta = node.NodeSize == AtlasNodeSize.Big ? bigNodeSize : smallNodeSize;
        }

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
            button.onClick.AddListener(HandleClicked);
        }
    }

    public void SetState(bool isAllocated, bool canAllocate, bool canRefund)
    {
        if (frameImage != null)
        {
            frameImage.color = isAllocated
                ? (canRefund ? refundColor : allocatedColor)
                : (canAllocate ? availableColor : lockedColor);
        }

        if (nameText != null)
        {
            nameText.color = isAllocated || canAllocate ? enabledTextColor : disabledTextColor;
        }

        if (button != null)
        {
            button.interactable = isAllocated ? canRefund : canAllocate;
        }
    }

    private void HandleClicked()
    {
        if (boundNode == null)
        {
            return;
        }

        clickHandler?.Invoke(boundNode);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (boundNode == null)
        {
            return;
        }

        hoverEnterHandler?.Invoke(boundNode);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverExitHandler?.Invoke();
    }
}
