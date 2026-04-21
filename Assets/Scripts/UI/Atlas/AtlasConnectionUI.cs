using UnityEngine;

public class AtlasConnectionUI : MonoBehaviour
{
    [SerializeField] private RectTransform lineTransform;
    [SerializeField, Min(1f)] private float lineThickness = 6f;

    // Draws a simple UI line between two node positions in tree-local space.
    public void SetEndpoints(Vector2 from, Vector2 to)
    {
        RectTransform targetTransform = lineTransform != null ? lineTransform : transform as RectTransform;
        if (targetTransform == null)
        {
            return;
        }

        Vector2 delta = to - from;
        float length = delta.magnitude;

        targetTransform.anchorMin = new Vector2(0.5f, 0.5f);
        targetTransform.anchorMax = new Vector2(0.5f, 0.5f);
        targetTransform.pivot = new Vector2(0f, 0.5f);
        targetTransform.anchoredPosition = from;
        targetTransform.sizeDelta = new Vector2(length, lineThickness);
        targetTransform.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }
}
