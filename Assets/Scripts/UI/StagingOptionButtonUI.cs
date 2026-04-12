using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StagingOptionButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    private Action onClick;
    private Action onHoverEnter;
    private Action onHoverExit;

    public void Setup(string label, Sprite icon, Action clickCallback, Action hoverEnterCallback, Action hoverExitCallback)
    {
        onClick = clickCallback;
        onHoverEnter = hoverEnterCallback;
        onHoverExit = hoverExitCallback;

        nameText.text = label;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverExit?.Invoke();
    }
}
