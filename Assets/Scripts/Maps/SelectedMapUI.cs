using TMPro;
using UnityEngine;

public class SelectedMapUI : MonoBehaviour
{
    [SerializeField] private TMP_Text infoText;

    public void SetMap(MapInstance map)
    {
        if (map == null)
        {
            Clear();
            return;
        }

        infoText.text = MapDescriptionFormatter.Build(map);
    }

    public void Clear()
    {
        infoText.text = "No Map Selected";
    }
}
