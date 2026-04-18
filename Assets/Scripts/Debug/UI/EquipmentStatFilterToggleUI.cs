using UnityEngine;
using UnityEngine.UI;

public class EquipmentStatFilterToggleUI : MonoBehaviour
{
    [SerializeField] private EquipmentStatType statType;
    [SerializeField] private Toggle toggle;

    public EquipmentStatType StatType => statType;
    public bool IsSelected => toggle != null && toggle.isOn;

    public void SetSelected(bool selected)
    {
        if (toggle != null)
        {
            toggle.isOn = selected;
        }
    }
}
