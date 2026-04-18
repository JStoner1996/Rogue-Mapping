using UnityEngine;
using System.Collections.Generic;

public static class TargetingUtils
{
    public static Enemy FindNearestEnemy(Vector3 position, float range, HashSet<Enemy> ignoreList = null)
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>();

        Enemy nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            if (ignoreList != null && ignoreList.Contains(enemy))
                continue;

            float dist = Vector3.Distance(position, enemy.transform.position);

            if (dist < minDist && dist <= range)
            {
                minDist = dist;
                nearest = enemy;
            }
        }

        return nearest;
    }
}
