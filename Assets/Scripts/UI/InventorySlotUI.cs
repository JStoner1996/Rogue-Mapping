using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDragDropTargetUI
{
    [Header("Components")]
    [SerializeField] private Button button;
    [SerializeField] private DraggableItemUI draggableItem;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private GameObject selectedOutline;
    [SerializeField] private Image selectedOutlineImage;
    [SerializeField] private GameObject discardOverlay;
    [SerializeField] private TMP_Text itemNameText;

    [Header("State Colors")]
    [SerializeField] private Color emptyBorderColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color filledBorderColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color selectedBorderColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color equippedOutlineColor = new Color(0.35f, 1f, 0.45f, 1f);
    [SerializeField] private Color selectedOutlineColor = new Color(1f, 0.25f, 0.25f, 1f);

    private InventorySlotViewData currentData;
    private Action<InventorySlotUI, InventorySlotViewData> onClick;
    private Action<InventorySlotUI, InventorySlotViewData> onRightClick;
    private Action<InventorySlotUI, InventorySlotViewData> onHoverEnter;
    private Action<InventorySlotUI, InventorySlotViewData> onHoverExit;
    private Func<InventorySlotUI, DragItemPayload, bool> canAcceptDrop;
    private Action<InventorySlotUI, DragItemPayload> onDropReceived;
    private bool isDropHovered;

    public InventorySlotViewData CurrentData => currentData;

    void Awake()
    {
        if (selectedOutlineImage == null && selectedOutline != null)
        {
            selectedOutlineImage = selectedOutline.GetComponent<Image>();
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }

        if (draggableItem != null)
        {
            draggableItem.ConfigurePayloadResolver(BuildDragPayload);
        }
    }

    public void Bind(
        InventorySlotViewData data,
        Action<InventorySlotUI, InventorySlotViewData> clickCallback = null,
        Action<InventorySlotUI, InventorySlotViewData> rightClickCallback = null,
        Action<InventorySlotUI, InventorySlotViewData> hoverEnterCallback = null,
        Action<InventorySlotUI, InventorySlotViewData> hoverExitCallback = null,
        Func<InventorySlotUI, DragItemPayload, bool> canAcceptDropCallback = null,
        Action<InventorySlotUI, DragItemPayload> dropReceivedCallback = null)
    {
        currentData = data ?? InventorySlotViewData.Empty();
        onClick = clickCallback;
        onRightClick = rightClickCallback;
        onHoverEnter = hoverEnterCallback;
        onHoverExit = hoverExitCallback;
        canAcceptDrop = canAcceptDropCallback;
        onDropReceived = dropReceivedCallback;
        isDropHovered = false;
        RefreshVisuals();
    }

    public void SetEmpty(Action<InventorySlotUI, InventorySlotViewData> clickCallback = null)
    {
        Bind(InventorySlotViewData.Empty(), clickCallback);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentData == null || currentData.isEmpty || !currentData.isInteractable)
        {
            return;
        }

        onHoverEnter?.Invoke(this, currentData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentData == null || currentData.isEmpty || !currentData.isInteractable)
        {
            return;
        }

        onHoverExit?.Invoke(this, currentData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        if (currentData == null || currentData.isEmpty || !currentData.isInteractable)
        {
            return;
        }

        onRightClick?.Invoke(this, currentData);
    }

    private void HandleClick()
    {
        if (currentData == null || currentData.isEmpty || !currentData.isInteractable)
        {
            return;
        }

        onClick?.Invoke(this, currentData);
    }

    private void RefreshVisuals()
    {
        bool isEmpty = currentData == null || currentData.isEmpty;
        bool hasIcon = !isEmpty && currentData.icon != null;

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(hasIcon);
            iconImage.sprite = hasIcon ? currentData.icon : null;
            iconImage.enabled = hasIcon;
        }

        if (itemNameText != null)
        {
            itemNameText.text = !isEmpty ? currentData.label : string.Empty;
            itemNameText.enabled = !string.IsNullOrWhiteSpace(itemNameText.text);
        }

        if (selectedOutline != null)
        {
            bool showOutline = !isEmpty && (currentData.isSelected || currentData.isEquipped);
            selectedOutline.SetActive(showOutline);

            if (showOutline && selectedOutlineImage != null)
            {
                selectedOutlineImage.color = currentData.isSelected
                    ? selectedOutlineColor
                    : equippedOutlineColor;
            }
        }

        if (discardOverlay != null)
        {
            discardOverlay.SetActive(!isEmpty && currentData.isDiscarded);
        }

        if (borderImage != null)
        {
            borderImage.color = isEmpty
                ? emptyBorderColor
                : isDropHovered || currentData.isFocused
                    ? selectedBorderColor
                    : filledBorderColor;
        }

        if (button != null)
        {
            button.interactable = !isEmpty && currentData.isInteractable;
        }

        if (draggableItem != null)
        {
            draggableItem.SetDragEnabled(!isEmpty && currentData.isInteractable && currentData.canDrag);
        }
    }

    private DragItemPayload BuildDragPayload()
    {
        if (currentData == null || currentData.isEmpty || !currentData.isInteractable || !currentData.canDrag)
        {
            return null;
        }

        return new DragItemPayload
        {
            itemId = currentData.id,
            label = currentData.label,
            icon = currentData.icon,
            itemType = currentData.dragItemType,
            sourceType = currentData.dragItemSourceType,
            hasEquipmentSlotType = currentData.hasEquipmentSlotType,
            equipmentSlotType = currentData.equipmentSlotType,
        };
    }

    public bool CanAcceptDrop(DragItemPayload payload)
    {
        return canAcceptDrop != null && canAcceptDrop.Invoke(this, payload);
    }

    public void OnDropReceived(DragItemPayload payload)
    {
        if (!CanAcceptDrop(payload))
        {
            return;
        }

        onDropReceived?.Invoke(this, payload);
    }

    public void OnDragHoverStart(DragItemPayload payload)
    {
        isDropHovered = CanAcceptDrop(payload);
        RefreshVisuals();
    }

    public void OnDragHoverEnd(DragItemPayload payload)
    {
        isDropHovered = false;
        RefreshVisuals();
    }
}
