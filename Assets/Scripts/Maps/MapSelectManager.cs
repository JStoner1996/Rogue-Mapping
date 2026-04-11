using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelectManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform buttonParent;
    [SerializeField] private MapButtonUI buttonPrefab;
    [SerializeField] private SelectedMapUI selectedMapUI;

    [Header("Generation")]
    [SerializeField] private int mapCount = 4;

    private List<GeneratedMap> generatedMaps = new List<GeneratedMap>();
    private GeneratedMap selectedMap;

    void Start()
    {
        GenerateMaps();
        CreateButtons();

        if (generatedMaps.Count > 0)
        {
            OnMapSelected(generatedMaps[0]);
        }
        else
        {
            selectedMapUI.Clear();
        }
    }

    private void GenerateMaps()
    {
        generatedMaps = MapGenerator.GenerateChoices(mapCount);
    }

    private void CreateButtons()
    {
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        foreach (GeneratedMap map in generatedMaps)
        {
            MapButtonUI button = Instantiate(buttonPrefab, buttonParent);
            button.Setup(map, OnMapSelected, PreviewMap, RestoreSelectedMapPreview);
        }
    }

    private void OnMapSelected(GeneratedMap map)
    {
        selectedMap = map;
        selectedMapUI.SetMap(map);
    }

    private void PreviewMap(GeneratedMap map)
    {
        selectedMapUI.SetMap(map);
    }

    private void RestoreSelectedMapPreview()
    {
        if (selectedMap != null)
        {
            selectedMapUI.SetMap(selectedMap);
            return;
        }

        selectedMapUI.Clear();
    }

    public void ConfirmSelection()
    {
        if (selectedMap == null)
        {
            Debug.LogWarning("No map selected!");
            return;
        }

        RunData.SelectedMap = selectedMap;
        SceneManager.LoadScene("Game");
    }
}
