using UnityEngine;

public class ScaleToRadius : MonoBehaviour
{
    [Tooltip("Diameter of this object when scale = 1. If 0, it will auto-detect.")]
    [SerializeField] private float baseSize = 0f;

    private bool initialized = false;

    void Awake()
    {
        AutoDetectBaseSize();
    }

    void AutoDetectBaseSize()
    {
        if (baseSize > 0f) return;

        var renderer = GetComponent<Renderer>();

        if (renderer != null)
        {
            baseSize = renderer.bounds.size.x;
            initialized = true;
        }
        else
        {
            Debug.LogWarning($"[ScaleToRadius] No Renderer found on {gameObject.name}. Cannot auto-detect size.");
        }
    }

    /// <summary>
    /// Scale object to match a radius (world units)
    /// </summary>
    public void SetRadius(float radius)
    {
        EnsureInitialized();

        float diameter = radius * 2f;
        SetDiameter(diameter);
    }

    /// <summary>
    /// Scale object to match a diameter (world units)
    /// </summary>
    public void SetDiameter(float diameter)
    {
        EnsureInitialized();

        if (baseSize <= 0f)
        {
            Debug.LogWarning($"[ScaleToRadius] Invalid baseSize on {gameObject.name}");
            return;
        }

        float scaleMultiplier = diameter / baseSize;
        transform.localScale = Vector3.one * scaleMultiplier;
    }

    void EnsureInitialized()
    {
        if (!initialized)
        {
            AutoDetectBaseSize();
        }
    }
}