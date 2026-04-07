using UnityEngine;

public static class DebugUtils
{
    public static void DrawCircle(Vector3 center, float radius, Color color, float duration = 0f, int segments = 30)
    {
        float angleStep = 360f / segments;

        Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;

            Vector3 nextPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            Debug.DrawLine(previousPoint, nextPoint, color, duration);
            previousPoint = nextPoint;
        }
    }

    public static void DrawBox(Vector2 center, Vector2 size, float angle, Color color, float duration = 0f)
    {
        Vector2 half = size / 2f;

        Vector2[] corners = new Vector2[4];
        corners[0] = new Vector2(-half.x, -half.y);
        corners[1] = new Vector2(-half.x, half.y);
        corners[2] = new Vector2(half.x, half.y);
        corners[3] = new Vector2(half.x, -half.y);

        Quaternion rot = Quaternion.Euler(0, 0, angle);

        for (int i = 0; i < 4; i++)
        {
            Vector2 p1 = center + (Vector2)(rot * corners[i]);
            Vector2 p2 = center + (Vector2)(rot * corners[(i + 1) % 4]);

            Debug.DrawLine(p1, p2, color, duration);
        }
    }
}