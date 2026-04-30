using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalTierOneColor = Color.yellow;
    [SerializeField] private Color criticalTierTwoColor = new Color(1f, 0.45f, 0f, 1f);
    [SerializeField] private Color criticalTierThreeColor = Color.red;

    private float floatSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        floatSpeed = Random.Range(0.1f, 1.5f);
        Destroy(gameObject, 1);
    }

    void Update()
    {
        transform.position += Vector3.up * Time.deltaTime * floatSpeed;
    }

    public void SetValue(int value)
    {
        SetValue(value, isCritical: false, criticalDamageMultiplier: 1f);
    }

    public void SetValue(int value, bool isCritical, float criticalDamageMultiplier)
    {
        damageText.text = value.ToString();
        damageText.color = normalColor;

        if (!isCritical)
        {
            return;
        }

        CriticalVisualTier tier = GetCriticalVisualTier(criticalDamageMultiplier);
        damageText.text += new string('!', tier.exclamationCount);
        damageText.color = tier.color;
    }

    private CriticalVisualTier GetCriticalVisualTier(float criticalDamageMultiplier)
    {
        if (criticalDamageMultiplier >= 3.5f)
        {
            return new CriticalVisualTier(criticalTierThreeColor, 3);
        }

        if (criticalDamageMultiplier >= 2.5f)
        {
            return new CriticalVisualTier(criticalTierTwoColor, 2);
        }

        return new CriticalVisualTier(criticalTierOneColor, 1);
    }

    private readonly struct CriticalVisualTier
    {
        public CriticalVisualTier(Color color, int exclamationCount)
        {
            this.color = color;
            this.exclamationCount = exclamationCount;
        }

        public readonly Color color;
        public readonly int exclamationCount;
    }
}
