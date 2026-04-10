using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    [Header("Collector Settings")]
    [SerializeField] private CircleCollider2D collectorCollider;
    [SerializeField] private float pickupRadius = 2f;

    private void Awake()
    {
        if (collectorCollider == null)
        {
            TryGetComponent(out collectorCollider);
        }
    }

    private void Start()
    {
        collectorCollider.radius = pickupRadius;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out IItem item))
        {
            item.Collect();
        }
    }
}
