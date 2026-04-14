using System.Text;
using UnityEngine;

public class EquipmentDebugGenerator : MonoBehaviour
{
    [SerializeField] private EquipmentBaseCatalog baseCatalog;
    [SerializeField] private EquipmentAffixCatalog affixCatalog;
    [SerializeField] private EquipmentGenerationRequest request = new EquipmentGenerationRequest();
    [Header("Rarity Weights")]
    [SerializeField, Min(0f)] private float commonWeight = EquipmentGenerator.DefaultCommonWeight;
    [SerializeField, Min(0f)] private float uncommonWeight = EquipmentGenerator.DefaultUncommonWeight;
    [SerializeField, Min(0f)] private float rareWeight = EquipmentGenerator.DefaultRareWeight;

    public EquipmentInstance LastGeneratedItem { get; private set; }

    public void GenerateTestItem()
    {
        LastGeneratedItem = EquipmentGenerator.Generate(
            baseCatalog,
            affixCatalog,
            request,
            commonWeight,
            uncommonWeight,
            rareWeight);

        if (LastGeneratedItem == null)
        {
            Debug.LogWarning("Equipment test generation failed.");
            return;
        }

        Debug.Log(BuildDebugDescription(LastGeneratedItem));
    }

    private static string BuildDebugDescription(EquipmentInstance item)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Generated Item: {item.DisplayName}");
        builder.AppendLine($"Rarity: {item.Rarity}");
        builder.AppendLine($"Tier: {item.ItemTier}");
        builder.AppendLine($"Slot: {item.SlotType}");

        AppendRolls(builder, "Implicit", item.ImplicitRolls);
        AppendAffixes(builder, "Prefix", item.PrefixAffixes);
        AppendAffixes(builder, "Suffix", item.SuffixAffixes);

        return builder.ToString();
    }

    private static void AppendRolls(StringBuilder builder, string sectionName, System.Collections.Generic.IReadOnlyList<EquipmentModifierRoll> rolls)
    {
        if (rolls == null || rolls.Count == 0)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(sectionName))
        {
            builder.AppendLine($"{sectionName}:");
        }

        for (int i = 0; i < rolls.Count; i++)
        {
            EquipmentModifierRoll roll = rolls[i];
            string formattedValue = roll.modifierKind == EquipmentModifierKind.Percent
                ? $"+{roll.value * 100f:F1}%"
                : $"+{roll.value:F1}";

            builder.AppendLine($"- {roll.statType}: {formattedValue}");
        }
    }

    private static void AppendAffixes(StringBuilder builder, string sectionName, System.Collections.Generic.IReadOnlyList<EquipmentRolledAffix> affixes)
    {
        if (affixes == null || affixes.Count == 0)
        {
            return;
        }

        for (int i = 0; i < affixes.Count; i++)
        {
            EquipmentRolledAffix affix = affixes[i];
            if (affix == null)
            {
                continue;
            }

            builder.AppendLine($"{sectionName}: {affix.AffixName} (Tier {affix.AffixTier})");
            AppendRolls(builder, string.Empty, affix.ModifierRolls);
        }
    }
}
