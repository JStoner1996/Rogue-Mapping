using UnityEngine;

public class Pulse : MonoBehaviour
{
    [Header("Pulse Animation")]
    [SerializeField] private float minSpeed = 1.8f;
    [SerializeField] private float maxSpeed = 2.2f;
    [SerializeField] private float scaleAmount = 0.2f;

    private float speed;
    private Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
        speed = Random.Range(minSpeed, maxSpeed);
    }

    void Update()
    {
        float scale = 1 + Mathf.Sin(Time.time * speed) * scaleAmount;
        transform.localScale = startScale * scale;
    }
}