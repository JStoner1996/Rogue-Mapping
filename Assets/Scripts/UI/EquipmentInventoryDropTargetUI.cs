using System;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentInventoryDropTargetUI : MonoBehaviour, IDragDropTargetUI
{
    [SerializeField] private Image highlightImage;
    [SerializeField] private Color validHighlightColor = new Color(0.35f, 1f, 0.45f, 0.35f);

    public event Action<DragItemPayload> DropReceived;

    void Awake()
    {
        SetHighlight(false);
    }

    public bool CanAcceptDrop(DragItemPayload payload)
    {
        return payload != null && payload.itemType == DragItemType.Equipment;
    }

    public void OnDropReceived(DragItemPayload payload)
    {
        if (!CanAcceptDrop(payload))
        {
            return;
        }

        DropReceived?.Invoke(payload);
    }

    public void OnDragHoverStart(DragItemPayload payload)
    {
        SetHighlight(CanAcceptDrop(payload));
    }

    public void OnDragHoverEnd(DragItemPayload payload)
    {
        SetHighlight(false);
    }

    private void SetHighlight(bool visible)
    {
        if (highlightImage == null)
        {
            return;
        }

        highlightImage.gameObject.SetActive(visible);
        highlightImage.enabled = visible;

        if (visible)
        {
            highlightImage.color = validHighlightColor;
        }
    }
}
