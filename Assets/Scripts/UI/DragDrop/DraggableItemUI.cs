using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private bool dragEnabled = true;

    private Func<DragItemPayload> payloadResolver;

    public void ConfigurePayloadResolver(Func<DragItemPayload> resolver)
    {
        payloadResolver = resolver;
    }

    public void SetDragEnabled(bool enabled)
    {
        dragEnabled = enabled;
    }

    private DragItemPayload ResolveDragPayload()
    {
        DragItemPayload payload = payloadResolver?.Invoke();
        return payload != null && payload.IsValid() ? payload : null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!dragEnabled || DragDropManagerUI.Instance == null)
        {
            return;
        }

        DragItemPayload payload = ResolveDragPayload();
        if (payload == null)
        {
            return;
        }

        DragDropManagerUI.Instance.BeginDrag(payload);
        DragDropManagerUI.Instance.EvaluateHover(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragEnabled || DragDropManagerUI.Instance == null || !DragDropManagerUI.Instance.IsDragging)
        {
            return;
        }

        DragDropManagerUI.Instance.UpdateDrag(eventData);
        DragDropManagerUI.Instance.EvaluateHover(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragEnabled || DragDropManagerUI.Instance == null)
        {
            return;
        }

        DragDropManagerUI.Instance.EndDrag(eventData);
    }
}
