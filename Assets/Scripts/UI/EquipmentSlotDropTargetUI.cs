using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentSlotDropTargetUI : MonoBehaviour, IDragDropTargetUI, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private string loadoutSlotId;
    [SerializeField] private EquipmentSlotType slotType;
    [SerializeField] private Image highlightImage;
    [SerializeField] private Image equippedItemIcon;
    [SerializeField] private Color validHighlightColor = new Color(1f, 0.85f, 0.2f, 0.8f);
    [SerializeField] private Color equippedHighlightColor = new Color(0.35f, 1f, 0.45f, 0.8f);

    private bool isEquipped;
    private EquipmentInstance displayedEquipment;

    public string LoadoutSlotId => string.IsNullOrWhiteSpace(loadoutSlotId) ? slotType.ToString() : loadoutSlotId;
    public EquipmentSlotType SlotType => slotType;

    public event Action<EquipmentSlotDropTargetUI, DragItemPayload> DropReceived;
    public event Action<EquipmentSlotDropTargetUI> RightClicked;

    void Awake()
    {
        SetHighlight(false);
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
        SetHighlight(CanAcceptDrop(payload), validHighlightColor);
    }

    public void OnDragHoverEnd(DragItemPayload payload)
    {
        RefreshHighlight();
    }

    public void SetEquippedVisual(bool isEquipped)
    {
        this.isEquipped = isEquipped;
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
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            RightClicked?.Invoke(this);
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

    private void SetHighlight(bool visible, Color? colorOverride = null)
    {
        if (highlightImage == null)
        {
            return;
        }

        highlightImage.gameObject.SetActive(visible);
        highlightImage.enabled = visible;

        if (visible)
        {
            highlightImage.color = colorOverride ?? validHighlightColor;
        }
    }

    private void RefreshHighlight()
    {
        SetHighlight(isEquipped, equippedHighlightColor);
    }

    private void RefreshEquippedIcon(EquipmentInstance equipment)
    {
        if (equippedItemIcon == null)
        {
            return;
        }

        equippedItemIcon.sprite = equipment != null ? equipment.Icon : null;
        equippedItemIcon.gameObject.SetActive(equippedItemIcon.sprite != null);
        equippedItemIcon.enabled = equippedItemIcon.sprite != null;
    }
}
