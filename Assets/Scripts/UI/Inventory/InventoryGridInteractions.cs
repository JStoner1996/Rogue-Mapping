using System;

public class InventoryGridInteractions
{
    public Action<int, InventorySlotModel> OnSlotClicked;
    public Action<int, InventorySlotModel> OnSlotRightClicked;
    public Action<int, InventorySlotModel> OnSlotHoverEnter;
    public Action<int, InventorySlotModel> OnSlotHoverExit;
    public Func<int, DragItemPayload, bool> CanAcceptDropAtIndex;
    public Action<int, DragItemPayload> OnSlotDropReceived;

    public static InventoryGridInteractions Empty()
    {
        return new InventoryGridInteractions();
    }
}
