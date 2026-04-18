using UnityEngine;

public abstract class SingletonBehaviour<T> : MonoBehaviour
    where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    // Keeps a single live coordinator/service instance and optionally preserves it across scene loads.
    protected bool TryInitializeSingleton(bool persistAcrossScenes = false)
    {
        T currentInstance = this as T;

        if (currentInstance == null)
        {
            Debug.LogError($"{GetType().Name} could not initialize its singleton instance.");
            return false;
        }

        if (Instance != null && Instance != currentInstance)
        {
            Destroy(gameObject);
            return false;
        }

        Instance = currentInstance;

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        return true;
    }

    protected virtual void OnDestroy()
    {
        T currentInstance = this as T;

        if (currentInstance != null && Instance == currentInstance)
        {
            Instance = null;
        }
    }
}
