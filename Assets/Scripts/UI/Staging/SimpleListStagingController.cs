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

    // Standardizes the simple staging tabs so they all load, choose a default item, and sync RunData the same way.
    protected void LoadItems(IEnumerable<T> sourceItems, Predicate<T> preferredSelection = null)
    {
        SetItems(sourceItems);
        selectedItem = FindItem(preferredSelection) ?? GetItemAtIndex(0);
        hoveredItem = null;
        OnSelectionChanged(selectedItem);
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
        hoveredItem = null;
        OnSelectionChanged(item);
        RefreshPresentation();
    }

    private T GetItemAtIndex(int index)
    {
        return index >= 0 && index < items.Count ? items[index] : null;
    }

    private void SetHoveredItem(T item)
    {
        hoveredItem = item;
        RefreshPresentation();
    }

    private void OnSlotClicked(int index, InventorySlotModel data)
    {
        T item = GetItemAtIndex(index);
        if (item == null)
        {
            return;
        }

        SetSelectedItem(item);
    }

    private void OnSlotHoverEnter(int index, InventorySlotModel data)
    {
        T item = GetItemAtIndex(index);
        if (item == null)
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

    private void RefreshPresentation()
    {
        RefreshPreview();
        RefreshGrid();
    }

    private T FindItem(Predicate<T> match)
    {
        if (match == null)
        {
            return null;
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (match(items[i]))
            {
                return items[i];
            }
        }

        return null;
    }

    protected abstract string GetItemId(T item);
    protected abstract string GetItemLabel(T item);
    protected abstract UnityEngine.Sprite GetItemIcon(T item);
    protected virtual void OnSelectionChanged(T item) { }
}
