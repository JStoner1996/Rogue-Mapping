using System;
using System.Collections.Generic;

// Determines where equipment items should appear in the inventory layout.
public static class EquipmentInventoryLayoutService
{
    private static readonly EquipmentSlotType[] PriorityOrder =
    {
        EquipmentSlotType.Head,
        EquipmentSlotType.Chest,
        EquipmentSlotType.Legs,
        EquipmentSlotType.Feet,
        EquipmentSlotType.Hands,
        EquipmentSlotType.Necklace,
        EquipmentSlotType.Ring,
        EquipmentSlotType.Ring,
    };

    public static bool IsEquipped(string equipmentInstanceId, EquipmentLoadoutData loadout)
    {
        if (string.IsNullOrWhiteSpace(equipmentInstanceId) || loadout?.equippedItems == null)
        {
            return false;
        }

        for (int i = 0; i < loadout.equippedItems.Count; i++)
        {
            EquipmentLoadoutSlot slot = loadout.equippedItems[i];
            if (slot != null && slot.equipmentInstanceId == equipmentInstanceId)
            {
                return true;
            }
        }

        return false;
    }

    public static EquipmentSlotDropTargetUI FindBestEquipTarget(
        EquipmentInstance equipment,
        IReadOnlyList<EquipmentSlotDropTargetUI> dropTargets,
        IReadOnlyList<EquipmentInstance> availableEquipment,
        Func<string, string> getEquippedItemId)
    {
        // Chooses an equip destination using the current priority rules:
        // empty slot > replace lower tier > replace lower rarity > first slot.
        if (equipment == null || dropTargets == null || getEquippedItemId == null)
        {
            return null;
        }

        List<EquipmentSlotDropTargetUI> candidates = new List<EquipmentSlotDropTargetUI>();

        for (int i = 0; i < dropTargets.Count; i++)
        {
            EquipmentSlotDropTargetUI dropTarget = dropTargets[i];
            if (dropTarget != null && dropTarget.SlotType == equipment.SlotType)
            {
                candidates.Add(dropTarget);
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        if (candidates.Count == 1)
        {
            return candidates[0];
        }

        EquipmentSlotDropTargetUI emptySlot = null;
        EquipmentSlotDropTargetUI bestLowerTierSlot = null;
        EquipmentInstance bestLowerTierItem = null;
        EquipmentSlotDropTargetUI bestLowerRaritySlot = null;
        EquipmentInstance bestLowerRarityItem = null;
        EquipmentSlotDropTargetUI firstSlot = candidates[0];

        for (int i = 0; i < candidates.Count; i++)
        {
            EquipmentSlotDropTargetUI candidate = candidates[i];
            string equippedItemId = getEquippedItemId(candidate.LoadoutSlotId);

            if (string.IsNullOrWhiteSpace(equippedItemId))
            {
                emptySlot = candidate;
                break;
            }

            EquipmentInstance equippedItem = EquipmentInstanceLookup.FindById(availableEquipment, equippedItemId);
            if (equippedItem == null)
            {
                emptySlot = candidate;
                break;
            }

            if (equippedItem.InstanceId == equipment.InstanceId)
            {
                continue;
            }

            if (equippedItem.ItemTier < equipment.ItemTier)
            {
                if (bestLowerTierItem == null
                    || equippedItem.ItemTier < bestLowerTierItem.ItemTier
                    || (equippedItem.ItemTier == bestLowerTierItem.ItemTier
                        && equippedItem.Rarity.CompareTo(bestLowerTierItem.Rarity) < 0))
                {
                    bestLowerTierSlot = candidate;
                    bestLowerTierItem = equippedItem;
                }
            }

            if (equippedItem.Rarity.CompareTo(equipment.Rarity) < 0)
            {
                if (bestLowerRarityItem == null
                    || equippedItem.Rarity.CompareTo(bestLowerRarityItem.Rarity) < 0
                    || (equippedItem.Rarity == bestLowerRarityItem.Rarity
                        && equippedItem.ItemTier < bestLowerRarityItem.ItemTier))
                {
                    bestLowerRaritySlot = candidate;
                    bestLowerRarityItem = equippedItem;
                }
            }
        }

        if (emptySlot != null)
        {
            return emptySlot;
        }

        if (bestLowerTierSlot != null)
        {
            return bestLowerTierSlot;
        }

        if (bestLowerRaritySlot != null)
        {
            return bestLowerRaritySlot;
        }

        return firstSlot;
    }

    public static List<string> BuildInventoryLayout(
        IReadOnlyList<string> previousLayout,
        IReadOnlyList<EquipmentInstance> availableEquipment,
        int minimumSlotCount,
        IReadOnlyList<EquipmentSlotDropTargetUI> dropTargets,
        Func<string, string> getEquippedItemId)
    {
        // Rebuilds the inventory so equipped items occupy the compact priority strip first,
        // while displaced and unequipped items keep their previous positions when possible.
        List<string> layout = new List<string>();
        int slotCount = Math.Max(Math.Max(minimumSlotCount, 8), availableEquipment != null ? availableEquipment.Count : 0);

        List<string> availableIds = new List<string>();
        HashSet<string> availableIdSet = new HashSet<string>();

        if (availableEquipment != null)
        {
            for (int i = 0; i < availableEquipment.Count; i++)
            {
                EquipmentInstance equipment = availableEquipment[i];
                if (equipment == null || string.IsNullOrWhiteSpace(equipment.InstanceId))
                {
                    continue;
                }

                if (availableIdSet.Add(equipment.InstanceId))
                {
                    availableIds.Add(equipment.InstanceId);
                }
            }
        }

        List<string> equippedPriorityIds = GetEquippedPriorityIds(dropTargets, getEquippedItemId);
        HashSet<string> equippedIdSet = new HashSet<string>(equippedPriorityIds);

        for (int i = 0; i < slotCount; i++)
        {
            string previousId = previousLayout != null && i < previousLayout.Count ? previousLayout[i] : string.Empty;
            layout.Add(availableIdSet.Contains(previousId) ? previousId : string.Empty);
        }

        for (int i = 0; i < layout.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(layout[i]) && equippedIdSet.Contains(layout[i]))
            {
                layout[i] = string.Empty;
            }
        }

        List<string> displacedIds = new List<string>();
        for (int i = 0; i < equippedPriorityIds.Count; i++)
        {
            EnsureSize(layout, i + 1);

            string displacedId = layout[i];
            if (!string.IsNullOrWhiteSpace(displacedId) && !equippedIdSet.Contains(displacedId))
            {
                displacedIds.Add(displacedId);
            }

            layout[i] = equippedPriorityIds[i];
        }

        for (int i = 0; i < displacedIds.Count; i++)
        {
            int nextEmptyIndex = FindNextEmptyIndex(layout);
            layout[nextEmptyIndex] = displacedIds[i];
        }

        HashSet<string> placedIds = new HashSet<string>();
        for (int i = 0; i < layout.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(layout[i]))
            {
                placedIds.Add(layout[i]);
            }
        }

        for (int i = 0; i < availableIds.Count; i++)
        {
            if (placedIds.Contains(availableIds[i]))
            {
                continue;
            }

            int nextEmptyIndex = FindNextEmptyIndex(layout);
            layout[nextEmptyIndex] = availableIds[i];
        }

        return layout;
    }
    private static List<string> GetEquippedPriorityIds(IReadOnlyList<EquipmentSlotDropTargetUI> dropTargets, Func<string, string> getEquippedItemId)
    {
        // Collects currently equipped ids in the same order the inventory strip should display them.
        List<string> equippedIds = new List<string>(PriorityOrder.Length);
        List<EquipmentSlotDropTargetUI> orderedTargets = new List<EquipmentSlotDropTargetUI>();

        if (dropTargets != null)
        {
            for (int i = 0; i < dropTargets.Count; i++)
            {
                if (dropTargets[i] != null)
                {
                    orderedTargets.Add(dropTargets[i]);
                }
            }
        }

        orderedTargets.Sort(CompareDropTargetPriority);

        for (int i = 0; i < orderedTargets.Count; i++)
        {
            string equippedItemId = getEquippedItemId(orderedTargets[i].LoadoutSlotId);
            if (!string.IsNullOrWhiteSpace(equippedItemId))
            {
                equippedIds.Add(equippedItemId);
            }
        }

        return equippedIds;
    }

    private static int CompareDropTargetPriority(EquipmentSlotDropTargetUI left, EquipmentSlotDropTargetUI right)
    {
        int leftPriority = GetDropTargetPriority(left);
        int rightPriority = GetDropTargetPriority(right);

        if (leftPriority != rightPriority)
        {
            return leftPriority.CompareTo(rightPriority);
        }

        string leftId = left != null ? left.LoadoutSlotId : string.Empty;
        string rightId = right != null ? right.LoadoutSlotId : string.Empty;
        return string.CompareOrdinal(leftId, rightId);
    }

    private static int GetDropTargetPriority(EquipmentSlotDropTargetUI dropTarget)
    {
        // Rings share a slot type, so their loadout id is used to keep Ring 1 ahead of Ring 2.
        if (dropTarget == null)
        {
            return int.MaxValue;
        }

        for (int i = 0; i < PriorityOrder.Length; i++)
        {
            if (PriorityOrder[i] != dropTarget.SlotType)
            {
                continue;
            }

            if (dropTarget.SlotType != EquipmentSlotType.Ring)
            {
                return i;
            }

            bool isRingTwo = dropTarget.LoadoutSlotId.IndexOf("2", StringComparison.OrdinalIgnoreCase) >= 0;
            return isRingTwo ? 7 : 6;
        }

        return int.MaxValue - 1;
    }

    private static int FindNextEmptyIndex(List<string> layout)
    {
        for (int i = 0; i < layout.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(layout[i]))
            {
                return i;
            }
        }

        layout.Add(string.Empty);
        return layout.Count - 1;
    }

    private static void EnsureSize(List<string> layout, int requiredSize)
    {
        while (layout.Count < requiredSize)
        {
            layout.Add(string.Empty);
        }
    }
}
