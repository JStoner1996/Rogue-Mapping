using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    private GeneratedMap map;
    private Action<GeneratedMap> onClick;
    private Action<GeneratedMap> onHoverEnter;
    private Action onHoverExit;

    public void Setup(GeneratedMap data, Action<GeneratedMap> clickCallback, Action<GeneratedMap> hoverEnterCallback, Action hoverExitCallback)
    {
        map = data;
        onClick = clickCallback;
        onHoverEnter = hoverEnterCallback;
        onHoverExit = hoverExitCallback;

        nameText.text = data.DisplayName;

        if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(map));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHoverEnter?.Invoke(map);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onHoverExit?.Invoke();
    }
}
