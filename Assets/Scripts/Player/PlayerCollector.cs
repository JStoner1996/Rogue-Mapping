using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    [Header("Collector Settings")]
    [SerializeField] private CircleCollider2D collectorCollider;
    [SerializeField] private float pickupRadius = 2f;
    private float pickupRangeMultiplier;

    public float PickupRadius => pickupRadius * (1f + pickupRangeMultiplier);

    private void Awake()
    {
        if (collectorCollider == null)
        {
            TryGetComponent(out collectorCollider);
        }
    }

    private void Start()
    {
        RefreshPickupRadius();
    }

    public void ApplyPickupRangeModifier(float value)
    {
        pickupRangeMultiplier += value;
        RefreshPickupRadius();
    }

    private void RefreshPickupRadius()
    {
        if (collectorCollider == null)
        {
            return;
        }

        collectorCollider.radius = PickupRadius;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out IItem item))
        {
            item.Collect();
        }
    }
}
