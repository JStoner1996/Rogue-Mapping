using System;
using System.Collections.Generic;

// Shared base for simple staging tabs that present a selectable list with hover preview.
public abstract class SimpleListStagingController<T> : IStagingTabController
    where T : class
{
    private readonly InventoryGridUI grid;
    private readonly Action<T> showPreview;
    private readonly List<T> items = new List<T>();

    private T selectedItem;
    private T hoveredItem;

    protected SimpleListStagingController(InventoryGridUI grid, Action<T> showPreview)
    {
        this.grid = grid;
        this.showPreview = showPreview;
    }

    protected IReadOnlyList<T> Items => items;
    protected T SelectedItem
    {
        get => selectedItem;
        set => selectedItem = value;
    }

    protected void SetItems(IEnumerable<T> sourceItems)
    {
        items.Clear();

        if (sourceItems == null)
        {
            return;
        }

        foreach (T item in sourceItems)
        {
            if (item != null)
            {
                items.Add(item);
            }
        }
    }

    protected void SetSelectedItem(T item)
    {
        selectedItem = item;
        hoveredItem = null;
        OnSelectionChanged(item);
        showPreview?.Invoke(item);
        RefreshGrid();
    }

    protected void ClearHoveredItem()
    {
        hoveredItem = null;
    }

    public void RefreshGrid()
    {
        if (grid == null)
        {
            return;
        }

        grid.SetItems(
            SimpleInventoryGridPresenter.BuildGridModel(
                items,
                grid.MaxSlots,
                selectedItem,
                hoveredItem,
                GetItemId,
                GetItemLabel,
                GetItemIcon),
            new InventoryGridInteractions
            {
                OnSlotClicked = OnSlotClicked,
                OnSlotHoverEnter = OnSlotHoverEnter,
                OnSlotHoverExit = OnSlotHoverExit,
            });
    }

    public void RefreshPreview()
    {
        showPreview?.Invoke(hoveredItem ?? selectedItem);
    }

    private void OnSlotClicked(int index, InventorySlotModel data)
    {
        if (index < 0 || index >= items.Count)
        {
            return;
        }

        SetSelectedItem(items[index]);
    }

    private void OnSlotHoverEnter(int index, InventorySlotModel data)
    {
        if (index < 0 || index >= items.Count)
        {
            return;
        }

        hoveredItem = items[index];
        showPreview?.Invoke(hoveredItem);
        RefreshGrid();
    }

    private void OnSlotHoverExit(int index, InventorySlotModel data)
    {
        hoveredItem = null;
        RefreshPreview();
        RefreshGrid();
    }

    protected abstract string GetItemId(T item);
    protected abstract string GetItemLabel(T item);
    protected abstract UnityEngine.Sprite GetItemIcon(T item);
    protected virtual void OnSelectionChanged(T item) { }
}
