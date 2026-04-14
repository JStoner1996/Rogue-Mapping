using System.Collections.Generic;

public class EquipmentStatSummary
{
    private readonly Dictionary<EquipmentStatType, EquipmentStatSummaryEntry> entriesByStat =
        new Dictionary<EquipmentStatType, EquipmentStatSummaryEntry>();

    public IReadOnlyCollection<EquipmentStatSummaryEntry> Entries => entriesByStat.Values;

    public void AddRoll(EquipmentModifierRoll roll)
    {
        EquipmentStatSummaryEntry entry = GetOrCreateEntry(roll.statType);

        switch (roll.modifierKind)
        {
            case EquipmentModifierKind.Flat:
                entry.flatValue += roll.value;
                break;

            case EquipmentModifierKind.Percent:
                entry.percentValue += roll.value;
                break;
        }
    }

    public EquipmentStatSummaryEntry GetEntry(EquipmentStatType statType)
    {
        entriesByStat.TryGetValue(statType, out EquipmentStatSummaryEntry entry);
        return entry;
    }

    public bool HasAnyValue(EquipmentStatType statType)
    {
        return GetEntry(statType)?.HasAnyValue == true;
    }

    private EquipmentStatSummaryEntry GetOrCreateEntry(EquipmentStatType statType)
    {
        if (!entriesByStat.TryGetValue(statType, out EquipmentStatSummaryEntry entry))
        {
            entry = new EquipmentStatSummaryEntry(statType);
            entriesByStat.Add(statType, entry);
        }

        return entry;
    }
}
