using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class WeaponButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    private WeaponData weaponData;
    private Action<WeaponData> onClick;
    private Action<WeaponData> onHoverEnter;
    private Action onHoverExit;

    public void Setup(WeaponData data, Action<WeaponData> clickCallback, Action<WeaponData> hoverEnterCallback, Action hoverExitCallback)
    {
        weaponData = data;
        onClick = clickCallback;
        onHoverEnter = hoverEnterCallback;
        onHoverExit = hoverExitCallback;

        nameText.text = data.weaponName;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = data.icon != null;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(weaponData));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverEnter?.Invoke(weaponData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverExit?.Invoke();
    }
}
