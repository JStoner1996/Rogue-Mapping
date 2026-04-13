using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private GameObject selectedOutline;
    [SerializeField] private GameObject discardOverlay;
    [SerializeField] private TMP_Text itemNameText;

    [Header("State Colors")]
    [SerializeField] private Color emptyBorderColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color filledBorderColor = new Color(1f, 1f, 1f, 0.8f);
    [SerializeField] private Color selectedBorderColor = new Color(1f, 0.85f, 0.2f, 1f);

    private InventorySlotViewData currentData;
    private Action<InventorySlotUI, InventorySlotViewData> onClick;

    public InventorySlotViewData CurrentData => currentData;

    void Awake()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
    }

    public void Bind(InventorySlotViewData data, Action<InventorySlotUI, InventorySlotViewData> clickCallback = null)
    {
        currentData = data ?? InventorySlotViewData.Empty();
        onClick = clickCallback;
        RefreshVisuals();
    }

    public void SetEmpty(Action<InventorySlotUI, InventorySlotViewData> clickCallback = null)
    {
        Bind(InventorySlotViewData.Empty(), clickCallback);
    }

    private void HandleClick()
    {
        if (currentData == null || currentData.isEmpty || !currentData.isInteractable)
        {
            return;
        }

        onClick?.Invoke(this, currentData);
    }

    private void RefreshVisuals()
    {
        bool isEmpty = currentData == null || currentData.isEmpty;
        bool hasIcon = !isEmpty && currentData.icon != null;

        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(hasIcon);
            iconImage.sprite = hasIcon ? currentData.icon : null;
            iconImage.enabled = hasIcon;
        }

        if (itemNameText != null)
        {
            itemNameText.text = !isEmpty ? currentData.label : string.Empty;
            itemNameText.enabled = !string.IsNullOrWhiteSpace(itemNameText.text);
        }

        if (selectedOutline != null)
        {
            selectedOutline.SetActive(!isEmpty && currentData.isSelected);
        }

        if (discardOverlay != null)
        {
            discardOverlay.SetActive(!isEmpty && currentData.isDiscarded);
        }

        if (borderImage != null)
        {
            borderImage.color = isEmpty
                ? emptyBorderColor
                : currentData.isSelected
                    ? selectedBorderColor
                    : filledBorderColor;
        }

        if (button != null)
        {
            button.interactable = !isEmpty && currentData.isInteractable;
        }
    }
}
