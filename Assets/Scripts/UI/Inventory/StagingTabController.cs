using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Manages staging tab buttons, active panel visibility, and tab visuals.
public class StagingTabController
{
    public enum Tab
    {
        Weapons,
        Maps,
        Equipment,
    }

    private readonly GameObject weaponsPanel;
    private readonly GameObject mapsPanel;
    private readonly GameObject equipmentPanel;
    private readonly Button weaponsTabButton;
    private readonly Button mapsTabButton;
    private readonly Button equipmentTabButton;
    private readonly Color activeTabColor;
    private readonly Color inactiveTabColor;
    private readonly List<Button> tabButtons = new List<Button>();

    private Tab currentTab;

    public StagingTabController(
        GameObject weaponsPanel,
        GameObject mapsPanel,
        GameObject equipmentPanel,
        Button weaponsTabButton,
        Button mapsTabButton,
        Button equipmentTabButton,
        Color activeTabColor,
        Color inactiveTabColor)
    {
        this.weaponsPanel = weaponsPanel;
        this.mapsPanel = mapsPanel;
        this.equipmentPanel = equipmentPanel;
        this.weaponsTabButton = weaponsTabButton;
        this.mapsTabButton = mapsTabButton;
        this.equipmentTabButton = equipmentTabButton;
        this.activeTabColor = activeTabColor;
        this.inactiveTabColor = inactiveTabColor;
    }

    public Tab CurrentTab => currentTab;

    public void RegisterTabButtons(UnityAction showWeaponsTab, UnityAction showMapsTab, UnityAction showEquipmentTab)
    {
        tabButtons.Clear();
        AddTabButton(weaponsTabButton, showWeaponsTab);
        AddTabButton(mapsTabButton, showMapsTab);
        AddTabButton(equipmentTabButton, showEquipmentTab);
    }

    public void SwitchTab(Tab tab, Action<Tab> onTabChanged = null)
    {
        currentTab = tab;
        RefreshPanelVisibility();
        RefreshTabVisuals();
        onTabChanged?.Invoke(tab);
    }

    private void AddTabButton(Button button, UnityAction onClick)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(onClick);
        button.onClick.AddListener(onClick);
        tabButtons.Add(button);
    }

    private void RefreshPanelVisibility()
    {
        if (weaponsPanel != null)
        {
            weaponsPanel.SetActive(currentTab == Tab.Weapons);
        }

        if (mapsPanel != null)
        {
            mapsPanel.SetActive(currentTab == Tab.Maps);
        }

        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(currentTab == Tab.Equipment);
        }
    }

    private void RefreshTabVisuals()
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            bool isActive = i == (int)currentTab;
            Image image = tabButtons[i].GetComponent<Image>();

            if (image != null)
            {
                image.color = isActive ? activeTabColor : inactiveTabColor;
            }
        }
    }
}
