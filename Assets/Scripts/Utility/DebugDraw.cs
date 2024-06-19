using UnityEngine;

public static class DebugDraw
{
    public static void DrawColliderPolygon(Collider2D collider, bool isServer = false)
    {
        Color color = isServer ? Color.red : Color.yellow;

        if (collider is PolygonCollider2D polyCollider)
        {
            Vector2[] points = polyCollider.points;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 currentPoint = polyCollider.transform.TransformPoint(points[i]);
                Vector2 nextPoint = polyCollider.transform.TransformPoint(points[(i + 1) % points.Length]);
                Debug.DrawLine(currentPoint, nextPoint, color, 1.0f);
            }
        }
        else if (collider is BoxCollider2D boxCollider)
        {
            Vector2[] points = new Vector2[4];
            Vector2 size = boxCollider.size;
            Vector2 offset = boxCollider.offset;
            points[0] = boxCollider.transform.TransformPoint(new Vector2(-size.x / 2 + offset.x, -size.y / 2 + offset.y));
            points[1] = boxCollider.transform.TransformPoint(new Vector2(size.x / 2 + offset.x, -size.y / 2 + offset.y));
            points[2] = boxCollider.transform.TransformPoint(new Vector2(size.x / 2 + offset.x, size.y / 2 + offset.y));
            points[3] = boxCollider.transform.TransformPoint(new Vector2(-size.x / 2 + offset.x, size.y / 2 + offset.y));

            for (int i = 0; i < points.Length; i++)
            {
                Debug.DrawLine(points[i], points[(i + 1) % points.Length], color, 1.0f);
            }
        }
        else if (collider is CircleCollider2D circleCollider)
        {
            int segments = 20;
            float angleStep = 360.0f / segments;
            Vector2 center = circleCollider.transform.TransformPoint(circleCollider.offset);
            float radius = circleCollider.radius;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = Mathf.Deg2Rad * (i * angleStep);
                float angle2 = Mathf.Deg2Rad * ((i + 1) % segments * angleStep);
                Vector2 point1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
                Vector2 point2 = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;
                Debug.DrawLine(point1, point2, color, 1.0f);
            }
        }
    }
}
