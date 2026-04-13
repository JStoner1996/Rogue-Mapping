using System;
using System.Collections.Generic;

public static class RunLootService
{
    public static event Action LootChanged;

    private static readonly List<RunLootEntry> RunLoot = new List<RunLootEntry>();

    public static IReadOnlyList<RunLootEntry> Entries => RunLoot;

    public static void AddMap(MapInstance map)
    {
        if (map == null)
        {
            return;
        }

        RunLoot.Add(new RunLootEntry
        {
            id = Guid.NewGuid().ToString("N"),
            lootType = RunLootType.Map,
            displayName = map.DisplayName,
            icon = map.Icon,
            map = map,
            isDiscarded = false,
        });

        LootChanged?.Invoke();
    }

    public static void AddEquipment(EquipmentInstance equipment)
    {
        if (equipment == null)
        {
            return;
        }

        RunLoot.Add(new RunLootEntry
        {
            id = Guid.NewGuid().ToString("N"),
            lootType = RunLootType.Equipment,
            displayName = equipment.DisplayName,
            icon = equipment.Icon,
            equipment = equipment,
            isDiscarded = false,
        });

        LootChanged?.Invoke();
    }

    public static void ToggleDiscard(string entryId)
    {
        RunLootEntry entry = RunLoot.Find(item => item.id == entryId);

        if (entry == null)
        {
            return;
        }

        entry.isDiscarded = !entry.isDiscarded;
        LootChanged?.Invoke();
    }

    public static void CommitKeptLoot()
    {
        foreach (RunLootEntry entry in RunLoot)
        {
            if (entry == null || entry.isDiscarded)
            {
                continue;
            }

            switch (entry.lootType)
            {
                case RunLootType.Map:
                    MetaProgressionService.AddOwnedMap(entry.map);
                    break;

                case RunLootType.Equipment:
                    MetaProgressionService.AddOwnedEquipment(entry.equipment);
                    break;
            }
        }

        Clear();
    }

    public static void Clear()
    {
        RunLoot.Clear();
        LootChanged?.Invoke();
    }
}
