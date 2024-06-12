using UnityEngine;
using UnityEngine.Tilemaps;

public class DebugColliderVisualizer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        // Only draw when in play mode
        if (!Application.isPlaying)
            return;

        // Get all Collider2D components in the scene
        Collider2D[] colliders = FindObjectsOfType<Collider2D>();

        foreach (Collider2D collider in colliders)
        {
            Gizmos.color = collider.isTrigger ? Color.red : Color.green;

            if (collider is BoxCollider2D)
            {
                DrawBoxCollider2D((BoxCollider2D)collider);
            }
            else if (collider is CircleCollider2D)
            {
                DrawCircleCollider2D((CircleCollider2D)collider);
            }
            else if (collider is PolygonCollider2D)
            {
                DrawPolygonCollider2D((PolygonCollider2D)collider);
            }
            else if (collider is CapsuleCollider2D)
            {
                DrawCapsuleCollider2D((CapsuleCollider2D)collider);
            }
            else if (collider is CompositeCollider2D)
            {
                DrawCompositeCollider2D((CompositeCollider2D)collider);
            }
        }
    }

    private void DrawBoxCollider2D(BoxCollider2D boxCollider)
    {
        Gizmos.matrix = Matrix4x4.TRS(boxCollider.transform.position, boxCollider.transform.rotation, boxCollider.transform.lossyScale);
        Gizmos.DrawWireCube((Vector3)boxCollider.offset, boxCollider.size);
    }

    private void DrawCircleCollider2D(CircleCollider2D circleCollider)
    {
        Gizmos.matrix = Matrix4x4.TRS(circleCollider.transform.position, circleCollider.transform.rotation, circleCollider.transform.lossyScale);
        Gizmos.DrawWireSphere((Vector3)circleCollider.offset, circleCollider.radius);
    }

    private void DrawPolygonCollider2D(PolygonCollider2D polygonCollider)
    {
        Gizmos.matrix = Matrix4x4.TRS(polygonCollider.transform.position, polygonCollider.transform.rotation, polygonCollider.transform.lossyScale);
        Vector2[] points = polygonCollider.points;

        for (int i = 0; i < points.Length; i++)
        {
            Vector2 currentPoint = points[i];
            Vector2 nextPoint = points[(i + 1) % points.Length];
            Gizmos.DrawLine((Vector3)currentPoint, (Vector3)nextPoint);
        }
    }

    private void DrawCapsuleCollider2D(CapsuleCollider2D capsuleCollider)
    {
        Gizmos.matrix = Matrix4x4.TRS(capsuleCollider.transform.position, capsuleCollider.transform.rotation, capsuleCollider.transform.lossyScale);

        float radius, height;
        Vector3 offset = (Vector3)capsuleCollider.offset;

        if (capsuleCollider.direction == CapsuleDirection2D.Vertical)
        {
            radius = capsuleCollider.size.x / 2;
            height = capsuleCollider.size.y;

            // Draw capsule body
            Vector3 bodySize = new Vector3(radius * 2, height - radius * 2, 0);
            Gizmos.DrawWireCube(offset, bodySize);

            // Draw capsule ends
            Gizmos.DrawWireSphere(offset + new Vector3(0, (height - radius * 2) / 2, 0), radius);
            Gizmos.DrawWireSphere(offset - new Vector3(0, (height - radius * 2) / 2, 0), radius);
        }
        else
        {
            radius = capsuleCollider.size.y / 2;
            height = capsuleCollider.size.x;

            // Draw capsule body
            Vector3 bodySize = new Vector3(height - radius * 2, radius * 2, 0);
            Gizmos.DrawWireCube(offset, bodySize);

            // Draw capsule ends
            Gizmos.DrawWireSphere(offset + new Vector3((height - radius * 2) / 2, 0, 0), radius);
            Gizmos.DrawWireSphere(offset - new Vector3((height - radius * 2) / 2, 0, 0), radius);
        }
    }

    private void DrawCompositeCollider2D(CompositeCollider2D compositeCollider)
    {
        Gizmos.matrix = Matrix4x4.TRS(compositeCollider.transform.position, compositeCollider.transform.rotation, compositeCollider.transform.lossyScale);

        for (int i = 0; i < compositeCollider.pathCount; i++)
        {
            Vector2[] points = new Vector2[compositeCollider.GetPathPointCount(i)];
            compositeCollider.GetPath(i, points);

            for (int j = 0; j < points.Length; j++)
            {
                Vector2 currentPoint = points[j];
                Vector2 nextPoint = points[(j + 1) % points.Length];
                Gizmos.DrawLine((Vector3)currentPoint, (Vector3)nextPoint);
            }
        }
    }
}
