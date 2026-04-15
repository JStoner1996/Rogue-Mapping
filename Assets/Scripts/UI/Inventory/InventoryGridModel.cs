using System.Collections.Generic;

public class InventoryGridModel
{
    private readonly List<InventorySlotModel> slots;

    public IReadOnlyList<InventorySlotModel> Slots => slots;
    public int SlotCount => slots.Count;

    public InventoryGridModel(int slotCount)
    {
        slots = new List<InventorySlotModel>(slotCount);

        for (int i = 0; i < slotCount; i++)
        {
            slots.Add(InventorySlotModel.Empty());
        }
    }

    public InventoryGridModel(IReadOnlyList<InventorySlotModel> slotModels, int minimumSlotCount = 0)
    {
        int slotCount = minimumSlotCount > 0 ? minimumSlotCount : (slotModels != null ? slotModels.Count : 0);
        slots = new List<InventorySlotModel>(slotCount);

        for (int i = 0; i < slotCount; i++)
        {
            InventorySlotModel slotModel = slotModels != null && i < slotModels.Count && slotModels[i] != null
                ? slotModels[i]
                : InventorySlotModel.Empty();

            slots.Add(slotModel);
        }
    }

    public InventorySlotModel GetSlot(int index)
    {
        return index >= 0 && index < slots.Count ? slots[index] : null;
    }
}
