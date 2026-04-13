using System;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotDropTargetUI : MonoBehaviour, IDragDropTargetUI
{
    [SerializeField] private string loadoutSlotId;
    [SerializeField] private EquipmentSlotType slotType;
    [SerializeField] private Image highlightImage;
    [SerializeField] private Image equippedItemIcon;
    [SerializeField] private Color validHighlightColor = new Color(1f, 0.85f, 0.2f, 0.8f);
    [SerializeField] private Color equippedHighlightColor = new Color(0.35f, 1f, 0.45f, 0.8f);

    private bool isEquipped;

    public string LoadoutSlotId => string.IsNullOrWhiteSpace(loadoutSlotId) ? slotType.ToString() : loadoutSlotId;
    public EquipmentSlotType SlotType => slotType;

    public event Action<EquipmentSlotDropTargetUI, DragItemPayload> DropReceived;

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
        isEquipped = equipment != null;
        RefreshHighlight();
        RefreshEquippedIcon(equipment);
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
