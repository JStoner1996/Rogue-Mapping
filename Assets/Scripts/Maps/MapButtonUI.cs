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

    private MapInstance map;
    private Action<MapInstance> onClick;
    private Action<MapInstance> onHoverEnter;
    private Action onHoverExit;

    public void Setup(MapInstance data, Action<MapInstance> clickCallback, Action<MapInstance> hoverEnterCallback, Action hoverExitCallback)
    {
        map = data;
        onClick = clickCallback;
        onHoverEnter = hoverEnterCallback;
        onHoverExit = hoverExitCallback;

        nameText.text = data.DisplayName;

        if (iconImage != null)
        {
            iconImage.sprite = data.Icon;
            iconImage.enabled = data.Icon != null;
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
