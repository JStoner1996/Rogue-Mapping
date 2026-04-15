using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RunCompletePanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text mapNameText;
    [SerializeField] private InventoryGridUI lootGrid;
    [SerializeField] private ItemDetailsPanelUI hoverPreviewUI;

    void OnEnable()
    {
        RunLootService.LootChanged += Refresh;
    }

    void OnDisable()
    {
        RunLootService.LootChanged -= Refresh;
    }

    public void Show(MapInstance completedMap)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (mapNameText != null)
        {
            mapNameText.text = completedMap != null ? completedMap.DisplayName : "Map Complete";
        }

        hoverPreviewUI?.HidePanel();
        Refresh();
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        hoverPreviewUI?.HidePanel();
    }

    public void Refresh()
    {
        if (lootGrid == null)
        {
            return;
        }

        IReadOnlyList<RunLootEntry> entries = RunLootService.Entries;
        List<InventorySlotModel> slotData = new List<InventorySlotModel>(entries.Count);

        foreach (RunLootEntry entry in entries)
        {
            if (entry == null)
            {
                continue;
            }

            slotData.Add(new InventorySlotModel
            {
                id = entry.id,
                label = entry.displayName,
                icon = entry.icon,
                isEmpty = false,
                isSelected = false,
                isDiscarded = entry.isDiscarded,
                isInteractable = true,
            });
        }

        lootGrid.SetItems(
            new InventoryGridModel(slotData, lootGrid.MaxSlots),
            new InventoryGridInteractions
            {
                OnSlotClicked = OnLootSlotClicked,
                OnSlotHoverEnter = OnLootSlotHoverEnter,
                OnSlotHoverExit = OnLootSlotHoverExit,
            });
    }

    public void OnCompleteRunPressed()
    {
        RunLootService.CommitKeptLoot();
        Hide();
        GameManager.Instance?.FinalizeCompletedRun();
    }

    private void OnLootSlotClicked(int index, InventorySlotModel data)
    {
        if (data == null || string.IsNullOrEmpty(data.id))
        {
            return;
        }

        RunLootService.ToggleDiscard(data.id);
    }

    private void OnLootSlotHoverEnter(int index, InventorySlotModel data)
    {
        if (hoverPreviewUI == null || index < 0 || index >= RunLootService.Entries.Count)
        {
            return;
        }

        RunLootEntry entry = RunLootService.Entries[index];

        if (entry == null)
        {
            return;
        }

        switch (entry.lootType)
        {
            case RunLootType.Map:
                hoverPreviewUI.ShowMap(entry.map);
                break;

            case RunLootType.Equipment:
                hoverPreviewUI.ShowEquipment(entry.equipment);
                break;
        }
    }

    private void OnLootSlotHoverExit(int index, InventorySlotModel data)
    {
        hoverPreviewUI?.HidePanel();
    }
}
