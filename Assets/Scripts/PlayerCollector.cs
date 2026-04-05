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
            collectorCollider = GetComponent<CircleCollider2D>();
        }
    }

    private void Start()
    {
        collectorCollider.radius = pickupRadius;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        IItem item = collider.GetComponent<IItem>();

        if (item != null)
        {
            item.Collect();
        }
    }
}