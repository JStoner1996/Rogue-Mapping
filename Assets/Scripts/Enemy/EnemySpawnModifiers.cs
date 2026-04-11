using UnityEngine;

[System.Serializable]
public class EnemySpawnModifiers
{
    [Min(0f)] public float quantity = 1f;
    [Min(0f)] public float quality = 1f;
}
