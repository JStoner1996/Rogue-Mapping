using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        AudioManager.EnsureExists();
    }
}