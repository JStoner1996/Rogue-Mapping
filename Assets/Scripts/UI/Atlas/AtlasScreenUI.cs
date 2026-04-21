using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AtlasScreenUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TMP_Text atlasPointsText;
    [SerializeField] private List<AtlasTreeUI> treeUis = new List<AtlasTreeUI>();

    void OnEnable()
    {
        InitializeTrees();
        Refresh();
    }

    void OnDisable()
    {
        HideNodeDetails();
    }

    public void Refresh()
    {
        if (atlasPointsText != null)
        {
            atlasPointsText.text = $"Atlas Points: {MetaProgressionService.UnspentAtlasPoints}";
        }

        foreach (AtlasTreeUI treeUI in treeUis)
        {
            treeUI?.Refresh();
        }
    }

    private void InitializeTrees()
    {
        foreach (AtlasTreeUI treeUI in treeUis)
        {
            treeUI?.Initialize(this);
        }
    }

    private void HideNodeDetails()
    {
        foreach (AtlasTreeUI treeUI in treeUis)
        {
            if (treeUI == null)
            {
                continue;
            }

            AtlasNodeDetailsPanelUI detailsPanel = treeUI.GetComponentInChildren<AtlasNodeDetailsPanelUI>(true);
            detailsPanel?.Hide();
        }
    }
}
