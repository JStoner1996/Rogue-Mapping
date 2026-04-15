using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class EquipmentSlotDropTargetUI : MonoBehaviour, IDragDropTargetUI, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Slot")]
    [SerializeField] private string loadoutSlotId;
    [SerializeField] private EquipmentSlotType slotType;

    [Header("Visual References")]
    [FormerlySerializedAs("hoverBorderImage")]
    [SerializeField] private Image slotBorderImage;
    [FormerlySerializedAs("highlightImage")]
    [SerializeField] private Image selectionOutlineImage;
    [FormerlySerializedAs("equippedItemIcon")]
    [SerializeField] private Image itemIconImage;

    [Header("Border Colors")]
    [FormerlySerializedAs("validHighlightColor")]
    [SerializeField] private Color hoveredBorderColor = new Color(1f, 0.85f, 0.2f, 0.8f);
    [FormerlySerializedAs("equippedHighlightColor")]
    [SerializeField] private Color equippedBorderColor = new Color(0.35f, 1f, 0.45f, 0.8f);
    [SerializeField] private Color filledBorderColor = new Color(1f, 1f, 1f, 0.8f);

    [Header("Selection Outline Colors")]
    [FormerlySerializedAs("selectedHighlightColor")]
    [SerializeField] private Color selectedOutlineColor = new Color(1f, 0.25f, 0.25f, 0.8f);

    private bool isEquipped;
    private bool isSelected;
    private bool isPointerHovered;
    private bool isLinkedHovered;
    private bool isDropHovered;
    private EquipmentInstance displayedEquipment;

    public string LoadoutSlotId => string.IsNullOrWhiteSpace(loadoutSlotId) ? slotType.ToString() : loadoutSlotId;
    public EquipmentSlotType SlotType => slotType;
    public EquipmentInstance DisplayedEquipment => displayedEquipment;

    public event Action<EquipmentSlotDropTargetUI, DragItemPayload> DropReceived;
    public event Action<EquipmentSlotDropTargetUI> RightClicked;
    public event Action<EquipmentSlotDropTargetUI> LeftClicked;
    public event Action<EquipmentSlotDropTargetUI> HoverEntered;
    public event Action<EquipmentSlotDropTargetUI> HoverExited;

    void Awake()
    {
        RefreshHighlight();
        RefreshEquippedIcon(null);
    }

    public bool CanAcceptDrop(DragItemPayload payload)
    {
        return payload != null
            && payload.itemType == DragItemType.Equipment
            && payload.hasEquipmentSlotType
            && payload.equipmentSlotType == slotType;
    }

    public void OnDropReceived(DragItemPayload payload)
    {
        if (!CanAcceptDrop(payload))
        {
            return;
        }

        DropReceived?.Invoke(this, payload);
    }

    public void OnDragHoverStart(DragItemPayload payload)
    {
        isDropHovered = CanAcceptDrop(payload);
        RefreshHighlight();
    }

    public void OnDragHoverEnd(DragItemPayload payload)
    {
        isDropHovered = false;
        RefreshHighlight();
    }

    public void SetEquippedVisual(bool isEquipped)
    {
        this.isEquipped = isEquipped;
        RefreshHighlight();
    }

    public void SetSelected(bool isSelected)
    {
        this.isSelected = isSelected;
        RefreshHighlight();
    }

    public void SetHovered(bool isHovered)
    {
        isLinkedHovered = isHovered;
        RefreshHighlight();
    }

    public void SetDisplayedEquipment(EquipmentInstance equipment)
    {
        displayedEquipment = equipment;
        isEquipped = equipment != null;
        RefreshHighlight();
        RefreshEquippedIcon(equipment);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            LeftClicked?.Invoke(this);
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            RightClicked?.Invoke(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerHovered = displayedEquipment != null;
        RefreshHighlight();

        if (isPointerHovered)
        {
            HoverEntered?.Invoke(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        bool wasHovered = isPointerHovered;
        isPointerHovered = false;
        RefreshHighlight();

        if (wasHovered)
        {
            HoverExited?.Invoke(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (displayedEquipment == null || DragDropManagerUI.Instance == null)
        {
            return;
        }

        DragItemPayload payload = new DragItemPayload
        {
            itemId = displayedEquipment.InstanceId,
            label = displayedEquipment.DisplayName,
            icon = displayedEquipment.Icon,
            itemType = DragItemType.Equipment,
            sourceType = DragItemSourceType.EquippedSlot,
            hasEquipmentSlotType = true,
            equipmentSlotType = displayedEquipment.SlotType,
        };

        DragDropManagerUI.Instance.BeginDrag(payload);
        DragDropManagerUI.Instance.EvaluateHover(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (displayedEquipment == null || DragDropManagerUI.Instance == null || !DragDropManagerUI.Instance.IsDragging)
        {
            return;
        }

        DragDropManagerUI.Instance.UpdateDrag(eventData);
        DragDropManagerUI.Instance.EvaluateHover(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (displayedEquipment == null || DragDropManagerUI.Instance == null)
        {
            return;
        }

        DragDropManagerUI.Instance.EndDrag(eventData);
    }

    private void SetBorderColor(Color color)
    {
        if (slotBorderImage == null)
        {
            return;
        }

        slotBorderImage.color = color;
    }

    private void SetSelectionOutline(bool visible, Color? colorOverride = null)
    {
        if (selectionOutlineImage == null)
        {
            return;
        }

        selectionOutlineImage.gameObject.SetActive(visible);
        selectionOutlineImage.enabled = visible;

        if (visible)
        {
            selectionOutlineImage.color = colorOverride ?? selectedOutlineColor;
        }
    }

    private void RefreshHighlight()
    {
        SetBorderColor(GetBorderColor());
        SetSelectionOutline(isSelected, selectedOutlineColor);
    }

    private Color GetBorderColor()
    {
        if (isPointerHovered || isLinkedHovered || isDropHovered)
        {
            return hoveredBorderColor;
        }

        if (isEquipped)
        {
            return equippedBorderColor;
        }

        return filledBorderColor;
    }

    private void RefreshEquippedIcon(EquipmentInstance equipment)
    {
        if (itemIconImage == null)
        {
            return;
        }

        itemIconImage.sprite = equipment != null ? equipment.Icon : null;
        itemIconImage.color = equipment != null ? Color.Lerp(Color.white, Color.red, Mathf.InverseLerp(1f, 10f, Mathf.Clamp(equipment.ItemTier, 1, 10))) : Color.white;
        itemIconImage.gameObject.SetActive(itemIconImage.sprite != null);
        itemIconImage.enabled = itemIconImage.sprite != null;
    }
}
