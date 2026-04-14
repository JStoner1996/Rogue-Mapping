using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDebugGenerationPanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_InputField minTierInput;
    [SerializeField] private TMP_InputField maxTierInput;
    [SerializeField] private TMP_InputField itemCountInput;
    [SerializeField] private Toggle forceSlotToggle;
    [SerializeField] private TMP_Dropdown forcedSlotDropdown;

    [Header("Rarity Weights")]
    [SerializeField] private TMP_InputField commonWeightInput;
    [SerializeField] private TMP_InputField uncommonWeightInput;
    [SerializeField] private TMP_InputField rareWeightInput;

    [Header("Stat Filters")]
    [SerializeField] private List<EquipmentStatFilterToggleUI> statFilters = new List<EquipmentStatFilterToggleUI>();

    [Header("Actions")]
    [SerializeField] private Button generateButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text feedbackText;

    void Awake()
    {
        PopulateSlotDropdown();
        RegisterButtons();
        ApplyDefaultValues();
        Hide();
    }

    public void Show()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }
    }

    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    public void GenerateTestItems()
    {
        EquipmentBaseCatalog baseCatalog = EquipmentCatalogResources.BaseCatalog;
        EquipmentAffixCatalog affixCatalog = EquipmentCatalogResources.AffixCatalog;

        if (baseCatalog == null || affixCatalog == null)
        {
            SetFeedback("Missing equipment catalogs.", Color.red);
            return;
        }

        EquipmentGenerationRequest request = BuildRequest();
        int itemCount = ParsePositiveInt(itemCountInput, 1);
        float commonWeight = ParseNonNegativeFloat(commonWeightInput, EquipmentGenerator.DefaultCommonWeight);
        float uncommonWeight = ParseNonNegativeFloat(uncommonWeightInput, EquipmentGenerator.DefaultUncommonWeight);
        float rareWeight = ParseNonNegativeFloat(rareWeightInput, EquipmentGenerator.DefaultRareWeight);

        int generatedCount = 0;

        for (int i = 0; i < itemCount; i++)
        {
            EquipmentInstance item = EquipmentGenerator.Generate(
                baseCatalog,
                affixCatalog,
                request,
                commonWeight,
                uncommonWeight,
                rareWeight);

            if (item == null)
            {
                continue;
            }

            MetaProgressionService.AddOwnedEquipment(item, false);
            generatedCount++;
        }

        if (generatedCount <= 0)
        {
            SetFeedback("No items generated.", Color.red);
            return;
        }

        MetaProgressionService.Save();
        RefreshStagingIfPresent();
        SetFeedback($"Generated {generatedCount} test item(s).", Color.green);
    }

    private EquipmentGenerationRequest BuildRequest()
    {
        EquipmentGenerationRequest request = new EquipmentGenerationRequest
        {
            minItemTier = ParsePositiveInt(minTierInput, 1),
            maxItemTier = ParsePositiveInt(maxTierInput, 1),
            forceSlotType = forceSlotToggle != null && forceSlotToggle.isOn,
            forcedSlotType = GetSelectedSlotType(),
        };

        for (int i = 0; i < statFilters.Count; i++)
        {
            EquipmentStatFilterToggleUI filter = statFilters[i];
            if (filter != null && filter.IsSelected)
            {
                request.requiredAffixStats.Add(filter.StatType);
            }
        }

        return request;
    }

    private void PopulateSlotDropdown()
    {
        if (forcedSlotDropdown == null)
        {
            return;
        }

        forcedSlotDropdown.ClearOptions();
        forcedSlotDropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(EquipmentSlotType))));
    }

    private void RegisterButtons()
    {
        if (generateButton != null)
        {
            generateButton.onClick.RemoveListener(GenerateTestItems);
            generateButton.onClick.AddListener(GenerateTestItems);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }
    }

    private void ApplyDefaultValues()
    {
        SetInputValue(minTierInput, "1");
        SetInputValue(maxTierInput, "10");
        SetInputValue(itemCountInput, "1");
        SetInputValue(commonWeightInput, EquipmentGenerator.DefaultCommonWeight.ToString("0"));
        SetInputValue(uncommonWeightInput, EquipmentGenerator.DefaultUncommonWeight.ToString("0"));
        SetInputValue(rareWeightInput, EquipmentGenerator.DefaultRareWeight.ToString("0"));
    }

    private void RefreshStagingIfPresent()
    {
        StagingManager stagingManager = FindObjectOfType<StagingManager>();
        if (stagingManager != null)
        {
            stagingManager.RefreshEquipmentDebugData();
        }
    }

    private EquipmentSlotType GetSelectedSlotType()
    {
        if (forcedSlotDropdown == null)
        {
            return EquipmentSlotType.Head;
        }

        return (EquipmentSlotType)Mathf.Clamp(forcedSlotDropdown.value, 0, forcedSlotDropdown.options.Count - 1);
    }

    private int ParsePositiveInt(TMP_InputField inputField, int fallback)
    {
        if (inputField == null || !int.TryParse(inputField.text, out int value))
        {
            return fallback;
        }

        return Mathf.Max(1, value);
    }

    private float ParseNonNegativeFloat(TMP_InputField inputField, float fallback)
    {
        if (inputField == null || !float.TryParse(inputField.text, out float value))
        {
            return fallback;
        }

        return Mathf.Max(0f, value);
    }

    private void SetInputValue(TMP_InputField inputField, string value)
    {
        if (inputField != null)
        {
            inputField.text = value;
        }
    }

    private void SetFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
    }
}
