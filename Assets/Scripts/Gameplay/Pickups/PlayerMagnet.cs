using UnityEngine;

public class PlayerMagnet : MonoBehaviour
{
    [SerializeField] private float duration = 5f;

    private float pullSpeed;
    private bool isActive;
    private float timer;

    public void ActivateMagnet(float speed)
    {
        pullSpeed = speed;
        isActive = true;
        timer = duration;
    }

    void Update()
    {
        if (!isActive)
        {
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isActive = false;
            return;
        }

        PullAllExpCrystals();
    }

    private void PullAllExpCrystals()
    {
        ExpCrystal[] crystals = FindObjectsByType<ExpCrystal>();

        foreach (ExpCrystal crystal in crystals)
        {
            Vector3 direction = (transform.position - crystal.transform.position).normalized;
            crystal.transform.position += direction * pullSpeed * Time.deltaTime;
        }
    }
}
