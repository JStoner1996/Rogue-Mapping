using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class WeaponButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    private WeaponData weaponData;
    private Action<WeaponData> onClick;

    public void Setup(WeaponData data, Action<WeaponData> callback)
    {
        weaponData = data;
        onClick = callback;

        nameText.text = data.weaponName;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = data.icon != null;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(weaponData));
    }
}