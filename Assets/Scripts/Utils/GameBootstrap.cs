using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private AudioManager audioManagerPrefab;

    void Awake()
    {
        if (AudioManager.Instance == null)
        {
            Instantiate(audioManagerPrefab);
        }
    }
}