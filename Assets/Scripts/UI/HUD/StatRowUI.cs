using TMPro;
using UnityEngine;

public class StatRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private string defaultLabel;

    public void SetLabel(string label)
    {
        if (labelText != null)
        {
            labelText.text = label;
        }
    }

    public void SetValue(string value, Color color)
    {
        if (valueText != null)
        {
            valueText.text = value;
            valueText.color = color;
        }
    }

    public void ResetToDefault()
    {
        if (!string.IsNullOrWhiteSpace(defaultLabel))
        {
            SetLabel(defaultLabel);
        }
    }
}
