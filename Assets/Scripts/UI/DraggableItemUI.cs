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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!dragEnabled || DragDropManagerUI.Instance == null)
        {
            return;
        }

        DragItemPayload payload = payloadResolver?.Invoke();

        if (payload == null || !payload.IsValid())
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
