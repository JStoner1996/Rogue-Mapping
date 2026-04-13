using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RunCompletePanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text mapNameText;
    [SerializeField] private InventoryGridUI lootGrid;

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

        Refresh();
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void Refresh()
    {
        if (lootGrid == null)
        {
            return;
        }

        IReadOnlyList<RunLootEntry> entries = RunLootService.Entries;
        List<InventorySlotViewData> slotData = new List<InventorySlotViewData>(entries.Count);

        foreach (RunLootEntry entry in entries)
        {
            if (entry == null)
            {
                continue;
            }

            slotData.Add(new InventorySlotViewData
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

        lootGrid.SetItems(slotData, OnLootSlotClicked);
    }

    public void OnCompleteRunPressed()
    {
        RunLootService.CommitKeptLoot();
        Hide();
        GameManager.Instance?.FinalizeCompletedRun();
    }

    private void OnLootSlotClicked(int index, InventorySlotViewData data)
    {
        if (data == null || string.IsNullOrEmpty(data.id))
        {
            return;
        }

        RunLootService.ToggleDiscard(data.id);
    }
}
