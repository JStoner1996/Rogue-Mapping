using UnityEngine;

public class MeasureSize : MonoBehaviour
{
    [ContextMenu("Log Size")]
    void LogSize()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Debug.Log($"{gameObject.name} Size: {renderer.bounds.size}");
        }
        else
        {
            Debug.LogWarning("No Renderer found.");
        }
    }
}