using UnityEngine;

public class Magnet : MonoBehaviour, IItem
{
    [SerializeField] private float pullSpeed = 5f;

    private PlayerMagnet playerMagnet;

    public void Collect()
    {
        if (playerMagnet == null && PlayerController.Instance != null)
        {
            playerMagnet = PlayerController.Instance.GetComponent<PlayerMagnet>();
        }

        playerMagnet.ActivateMagnet(pullSpeed);

        PickupPools.Instance.ReturnMagnet(this);
        AudioManager.Instance.Play(SoundType.Magnet);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }
}
