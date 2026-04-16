using UnityEngine;

// The shared data that gives a shrine its look, timing, and effect.
[CreateAssetMenu(fileName = "ShrineDefinition", menuName = "Shrines/Shrine Definition")]
public class ShrineDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string shrineName = "Shrine";

    [Header("Visuals")]
    [SerializeField] private Sprite shrineSprite;
    [SerializeField] private Color shrineTint = Color.white;
    [SerializeField] private Color areaTint = new Color(1f, 1f, 1f, 0.18f);

    [Header("Charge Timing")]
    [SerializeField, Min(0.1f)] private float chargeDuration = 5f;
    [SerializeField, Min(0.1f)] private float dischargeDuration = 2f;

    [Header("Effect")]
    [SerializeField] private ShrineEffectDefinition effect;

    public string ShrineName => string.IsNullOrWhiteSpace(shrineName) ? name : shrineName;
    public Sprite ShrineSprite => shrineSprite;
    public Color ShrineTint => shrineTint;
    public Color AreaTint => areaTint;
    public float ChargeDuration => chargeDuration;
    public float DischargeDuration => dischargeDuration;
    public ShrineEffectDefinition Effect => effect;
}
