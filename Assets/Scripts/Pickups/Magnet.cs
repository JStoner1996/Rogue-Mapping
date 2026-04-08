using UnityEngine;

public class Magnet : MonoBehaviour, IItem
{
    [SerializeField] private float pullSpeed = 5f;

    public void Collect()
    {
        PlayerController.Instance
            .GetComponent<PlayerMagnet>()
            .ActivateMagnet(pullSpeed);

        PickupPools.Instance.ReturnMagnet(this);
        AudioManager.Instance.Play(SoundType.Magnet);

    }
}