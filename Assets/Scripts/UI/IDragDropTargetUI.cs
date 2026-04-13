public interface IDragDropTargetUI
{
    bool CanAcceptDrop(DragItemPayload payload);
    void OnDropReceived(DragItemPayload payload);
    void OnDragHoverStart(DragItemPayload payload);
    void OnDragHoverEnd(DragItemPayload payload);
}
