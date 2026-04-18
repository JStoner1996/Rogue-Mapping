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

    protected void SetSelectedItem(T item)
    {
        selectedItem = item;
        ClearHoveredItem();
        OnSelectionChanged(item);
        showPreview?.Invoke(item);
        RefreshGrid();
    }

    private bool TryGetItemAtIndex(int index, out T item)
    {
        if (index >= 0 && index < items.Count)
        {
            item = items[index];
            return true;
        }

        item = null;
        return false;
    }

    private void SetHoveredItem(T item)
    {
        hoveredItem = item;
        showPreview?.Invoke(item ?? selectedItem);
        RefreshGrid();
    }

    private void OnSlotClicked(int index, InventorySlotModel data)
    {
        if (!TryGetItemAtIndex(index, out T item))
        {
            return;
        }

        SetSelectedItem(item);
    }

    private void OnSlotHoverEnter(int index, InventorySlotModel data)
    {
        if (!TryGetItemAtIndex(index, out T item))
        {
            return;
        }

        SetHoveredItem(item);
    }

    private void OnSlotHoverExit(int index, InventorySlotModel data)
    {
        SetHoveredItem(null);
    }

    protected void ClearHoveredItem()
    {
        hoveredItem = null;
    }

    protected abstract string GetItemId(T item);
    protected abstract string GetItemLabel(T item);
    protected abstract UnityEngine.Sprite GetItemIcon(T item);
    protected virtual void OnSelectionChanged(T item) { }
}
