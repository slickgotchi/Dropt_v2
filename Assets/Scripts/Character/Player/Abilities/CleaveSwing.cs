using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Mathematics;

public class CleaveSwing : PlayerAbility
{
    [Header("Cleave Swing Parameters")]
    [SerializeField] float Projection = 1f;

    private Animator m_animator;
    public float attackRange = 2.0f;
    public int damage = 10;
    public float hitFeedbackDuration = 0.1f; // Duration of the enemy flashing white

    private int m_enemyLayer;

    private Collider2D m_collider;


    private void Awake()
    {
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_animator = GetComponent<Animator>();
        m_enemyLayer = 1 << LayerMask.NameToLayer("EnemyHurt");
        m_collider = GetComponent<Collider2D>();
        
    }

    public override void OnStart()
    {
        // Play the attack animation
        if (IsClient)
        {
            m_animator.Play("CleaveSwing");
        }

        // update to acti
        SetRotationToDirection(PlayerActivationInput.actionDirection);
        SetPositionToPlayerCenterAtActivation(Projection * PlayerActivationInput.actionDirection);

        Physics2D.SyncTransforms();

        // do a collision check
        List<Collider2D> hitColliders = new List<Collider2D>();
        m_collider.Overlap(GetEnemyHurtContactFilter(), hitColliders);
        DrawColliderPolygon(m_collider, IsServer);
        foreach (var hit in hitColliders)
        {
            if (IsClient)
            {
                //Debug.Log("Hit on Client");
                if (hit.HasComponent<SpriteFlash>())
                {
                    hit.GetComponent<SpriteFlash>().DamageFlash();
                }
            }
            if (IsServer)
            {
                //Debug.Log("Hit on Server");
                hit.GetComponent<NetworkCharacter>().HpCurrent.Value -= 20;
            }
        }

        hitColliders.Clear();
    }

    private void Update()
    {
        SetPositionToPlayerCenter(PlayerActivationInput.actionDirection * Projection);
    }

    public override void OnFinish()
    {
    }


    private void DrawColliderPolygon(Collider2D collider, bool isServer = false)
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
