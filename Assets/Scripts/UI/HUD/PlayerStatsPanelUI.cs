using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsPanelUI : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;
    [SerializeField] private Button page1Button;
    [SerializeField] private Button page2Button;

    [Header("Page 1 Rows")]
    [Header("Offensive")]
    [SerializeField] private StatRowUI damageRow;
    [SerializeField] private StatRowUI attackSpeedRow;
    [SerializeField] private StatRowUI rangeRow;
    [Header("Defensive")]
    [SerializeField] private StatRowUI maxHealthRow;
    [SerializeField] private StatRowUI healthRegenRow;
    [SerializeField] private StatRowUI armorRow;
    [Header("Utility")]
    [SerializeField] private StatRowUI pickupRangeRow;
    [SerializeField] private StatRowUI shrineQuantityRow;
    [SerializeField] private StatRowUI mapDropChanceRow;
    [SerializeField] private StatRowUI equipmentDropChanceRow;
    [SerializeField] private StatRowUI experienceGainRow;
    [SerializeField] private StatRowUI enemyQuantityRow;
    [SerializeField] private StatRowUI enemyQualityRow;

    [Header("Page 2")]
    [SerializeField] private TMP_Text uniqueStatsText;
    [SerializeField] private string noUniqueStatsText = "No derived equipment stats yet.";

    [Header("Colors")]
    [SerializeField] private Color positiveColor = new Color(0.25f, 1f, 0.35f, 1f);
    [SerializeField] private Color negativeColor = new Color(1f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color neutralColor = Color.white;

    void Awake()
    {
        RegisterButtons();
        ShowPage1();
    }

    public void Refresh()
    {
        EquipmentStatSummary summary = MetaProgressionService.GetEquippedEquipmentStatSummary();

        SetPercentRows(
            summary,
            (damageRow, EquipmentStatType.Damage),
            (attackSpeedRow, EquipmentStatType.AttackSpeed),
            (rangeRow, EquipmentStatType.Range),
            (pickupRangeRow, EquipmentStatType.PickupRange),
            (shrineQuantityRow, EquipmentStatType.ShrineQuantity),
            (mapDropChanceRow, EquipmentStatType.MapDropChance),
            (equipmentDropChanceRow, EquipmentStatType.EquipmentDropChance),
            (experienceGainRow, EquipmentStatType.ExperienceGain),
            (enemyQuantityRow, EquipmentStatType.EnemyQuantity),
            (enemyQualityRow, EquipmentStatType.EnemyQuality));

        SetFlatWithOptionalPercentRow(maxHealthRow, GetEntry(summary, EquipmentStatType.MaximumHealth), "{0:+0;-0;+0}", " ({0:+0%;-0%;+0%})");
        SetFlatRow(healthRegenRow, GetEntry(summary, EquipmentStatType.HealthRegen), "{0:+0.##;-0.##;+0}/s");
        SetFlatRow(armorRow, GetEntry(summary, EquipmentStatType.Armor), "{0:+0.##;-0.##;+0}");

        if (page2Button != null)
        {
            page2Button.interactable = false;
        }

        if (uniqueStatsText != null)
        {
            uniqueStatsText.text = noUniqueStatsText;
            uniqueStatsText.color = neutralColor;
        }
    }

    public void ShowPage1()
    {
        SetPageVisibility(showPage1: true);
    }

    public void ShowPage2()
    {
        if (page2Button != null && !page2Button.interactable)
        {
            return;
        }

        SetPageVisibility(showPage1: false);
    }

    private void RegisterButtons()
    {
        if (page1Button != null)
        {
            page1Button.onClick.RemoveListener(ShowPage1);
            page1Button.onClick.AddListener(ShowPage1);
        }

        if (page2Button != null)
        {
            page2Button.onClick.RemoveListener(ShowPage2);
            page2Button.onClick.AddListener(ShowPage2);
        }
    }

    private EquipmentStatSummaryEntry GetEntry(EquipmentStatSummary summary, EquipmentStatType statType)
    {
        return summary?.GetEntry(statType);
    }

    private void SetPercentRow(StatRowUI row, EquipmentStatSummaryEntry entry)
    {
        float value = entry != null ? entry.percentValue : 0f;
        row?.SetValue(FormatPercent(value), GetColorForValue(value));
    }

    private void SetPercentRows(EquipmentStatSummary summary, params (StatRowUI row, EquipmentStatType statType)[] rows)
    {
        for (int i = 0; i < rows.Length; i++)
        {
            SetPercentRow(rows[i].row, GetEntry(summary, rows[i].statType));
        }
    }

    private void SetFlatRow(StatRowUI row, EquipmentStatSummaryEntry entry, string flatFormat)
    {
        float value = entry != null ? entry.flatValue : 0f;
        row?.SetValue(string.Format(flatFormat, value), GetColorForValue(value));
    }

    private void SetFlatWithOptionalPercentRow(StatRowUI row, EquipmentStatSummaryEntry entry, string flatFormat, string percentFormat)
    {
        float flatValue = entry != null ? entry.flatValue : 0f;
        float percentValue = entry != null ? entry.percentValue : 0f;

        string text = string.Format(flatFormat, flatValue);

        if (!Mathf.Approximately(percentValue, 0f))
        {
            text += string.Format(percentFormat, percentValue);
        }

        row?.SetValue(text, GetColorForValue(flatValue + percentValue));
    }

    private string FormatPercent(float value)
    {
        return $"{value:+0%;-0%;+0%}";
    }

    private void SetPageVisibility(bool showPage1)
    {
        if (page1 != null)
        {
            page1.SetActive(showPage1);
        }

        if (page2 != null)
        {
            page2.SetActive(!showPage1);
        }
    }

    private Color GetColorForValue(float value)
    {
        if (value > 0f)
        {
            return positiveColor;
        }

        if (value < 0f)
        {
            return negativeColor;
        }

        return neutralColor;
    }
}
