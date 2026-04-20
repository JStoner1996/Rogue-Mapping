using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDragDropTargetUI
{
    [Header("Interaction")]
    [SerializeField] private Button button;
    [SerializeField] private DraggableItemUI draggableItem;

    [Header("Visual References")]
    [FormerlySerializedAs("iconImage")]
    [SerializeField] private Image itemIconImage;
    [FormerlySerializedAs("borderImage")]
    [SerializeField] private Image slotBorderImage;
    [FormerlySerializedAs("selectedOutline")]
    [SerializeField] private GameObject selectionOutlineRoot;
    [FormerlySerializedAs("selectedOutlineImage")]
    [SerializeField] private Image selectionOutlineImage;
    [SerializeField] private GameObject discardOverlay;
    [SerializeField] private TMP_Text itemNameText;

    [Header("Border Colors")]
    [SerializeField] private Color emptyBorderColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color filledBorderColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color hoveredBorderColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color equippedBorderColor = new Color(0.35f, 1f, 0.45f, 1f);

    [Header("Selection Outline Colors")]
    [SerializeField] private Color selectedOutlineColor = new Color(1f, 0.25f, 0.25f, 1f);

    private InventorySlotModel currentData;
    private Action<InventorySlotUI, InventorySlotModel> onClick;
    private Action<InventorySlotUI, InventorySlotModel> onRightClick;
    private Action<InventorySlotUI, InventorySlotModel> onHoverEnter;
    private Action<InventorySlotUI, InventorySlotModel> onHoverExit;
    private Func<InventorySlotUI, DragItemPayload, bool> canAcceptDrop;
    private Action<InventorySlotUI, DragItemPayload> onDropReceived;
    private bool isDropHovered;

    public InventorySlotModel CurrentData => currentData;

    void Awake()
    {
        if (selectionOutlineImage == null && selectionOutlineRoot != null)
        {
            selectionOutlineImage = selectionOutlineRoot.GetComponent<Image>();
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
        InventorySlotModel data,
        Action<InventorySlotUI, InventorySlotModel> clickCallback = null,
        Action<InventorySlotUI, InventorySlotModel> rightClickCallback = null,
        Action<InventorySlotUI, InventorySlotModel> hoverEnterCallback = null,
        Action<InventorySlotUI, InventorySlotModel> hoverExitCallback = null,
        Func<InventorySlotUI, DragItemPayload, bool> canAcceptDropCallback = null,
        Action<InventorySlotUI, DragItemPayload> dropReceivedCallback = null)
    {
        currentData = data ?? InventorySlotModel.Empty();
        onClick = clickCallback;
        onRightClick = rightClickCallback;
        onHoverEnter = hoverEnterCallback;
        onHoverExit = hoverExitCallback;
        canAcceptDrop = canAcceptDropCallback;
        onDropReceived = dropReceivedCallback;
        isDropHovered = false;
        RefreshVisuals();
    }

    public void SetEmpty(Action<InventorySlotUI, InventorySlotModel> clickCallback = null)
    {
        Bind(InventorySlotModel.Empty(), clickCallback);
    }

    private bool CanInteractWithCurrentData() => currentData != null && !currentData.isEmpty && currentData.isInteractable;

    private void RefreshVisuals()
    {
        bool isEmpty = currentData == null || currentData.isEmpty;
        bool hasIcon = !isEmpty && currentData.icon != null;

        if (itemIconImage != null)
        {
            itemIconImage.gameObject.SetActive(hasIcon);
            itemIconImage.sprite = hasIcon ? currentData.icon : null;
            itemIconImage.enabled = hasIcon;
            itemIconImage.color = hasIcon ? currentData.iconTint : Color.white;
        }

        if (itemNameText != null)
        {
            itemNameText.text = !isEmpty ? currentData.label : string.Empty;
            itemNameText.enabled = !string.IsNullOrWhiteSpace(itemNameText.text);
        }

        bool showOutline = !isEmpty && currentData.isSelected;
        if (selectionOutlineRoot != null) selectionOutlineRoot.SetActive(showOutline);
        if (showOutline && selectionOutlineImage != null) selectionOutlineImage.color = selectedOutlineColor;
        if (discardOverlay != null) discardOverlay.SetActive(!isEmpty && currentData.isDiscarded);
        if (slotBorderImage != null) slotBorderImage.color = GetBorderColor(isEmpty);
        if (button != null) button.interactable = !isEmpty && currentData.isInteractable;
        if (draggableItem != null) draggableItem.SetDragEnabled(!isEmpty && currentData.isInteractable && currentData.canDrag);
    }

    private Color GetBorderColor(bool isEmpty) =>
        isEmpty ? emptyBorderColor
        : (isDropHovered || currentData.isHovered) ? hoveredBorderColor
        : currentData.isEquipped ? equippedBorderColor
        : filledBorderColor;

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanInteractWithCurrentData())
        {
            return;
        }

        onHoverEnter?.Invoke(this, currentData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!CanInteractWithCurrentData())
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

        if (!CanInteractWithCurrentData())
        {
            return;
        }

        onRightClick?.Invoke(this, currentData);
    }

    private void HandleClick()
    {
        if (!CanInteractWithCurrentData())
        {
            return;
        }

        onClick?.Invoke(this, currentData);
    }

    public bool CanAcceptDrop(DragItemPayload payload) => canAcceptDrop != null && canAcceptDrop.Invoke(this, payload);

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
