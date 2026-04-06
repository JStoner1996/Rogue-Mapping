using UnityEngine;

public static class TargetingUtils
{
    public static Enemy FindNearestEnemy(Vector3 origin, float range)
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>();

        Enemy nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(origin, enemy.transform.position);

            if (dist < minDist && dist <= range)
            {
                minDist = dist;
                nearest = enemy;
            }
        }

        return nearest;
    }
}