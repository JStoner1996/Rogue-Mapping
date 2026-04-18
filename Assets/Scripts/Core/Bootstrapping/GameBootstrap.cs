using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private AudioManager audioManagerPrefab;

    private void Awake()
    {
        if (AudioManager.Instance != null || audioManagerPrefab == null)
        {
            return;
        }

        Instantiate(audioManagerPrefab);
    }
}
