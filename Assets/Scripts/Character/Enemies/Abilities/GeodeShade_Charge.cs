using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GeodeShade_Charge : EnemyAbility
{
    [Header("GeodeShade_Charge Parameters")]
    public float ChargeDistance = 4f;

    private Vector3 m_direction;
    private float m_speed;
    private Collider2D m_collider;
    private bool m_isExecuting = false;

    private List<Transform> m_hitTransforms = new List<Transform>();

    private void Awake()
    {
        m_collider = GetComponent<Collider2D>();
    }

    public override void OnNetworkSpawn()
    {
    }

    public override void OnTelegraphStart()
    {
        transform.position = Parent.transform.position;
        m_direction = (Target.transform.position - Parent.transform.position).normalized;
        m_speed = ChargeDistance / ExecutionDuration;

        EnemyController.Facing facing = m_direction.x > 0 ? EnemyController.Facing.Right : EnemyController.Facing.Left;
        Parent.GetComponent<EnemyController>().SetFacing(facing);
    }

    public override void OnExecutionStart()
    {
        m_isExecuting = true;
    }

    public override void OnCooldownStart()
    {
        m_isExecuting = false;
    }

    public override void OnUpdate(float dt)
    {
        if (!m_isExecuting) return;

        ContinuousCollisionCheck();

        transform.position += m_direction * m_speed * Time.deltaTime;
        Parent.transform.position = transform.position;
    }

    public void ContinuousCollisionCheck()
    {
        Physics2D.SyncTransforms();

        // Use ColliderCast to perform continuous collision detection
        Vector2 castDirection = m_direction;
        float castDistance = m_speed * Time.deltaTime;
        RaycastHit2D[] rayHits = new RaycastHit2D[1];
        m_collider.Cast(castDirection,
            PlayerAbility.GetContactFilter(new string[] { "PlayerHurt", "Destructible" }),
            rayHits, castDistance);

        // loop through ray hits
        for (int i = 0; i < rayHits.Length; i++) 
        {
            var collider = rayHits[i].collider;
            if (collider == null) continue;
            var colliderTransform = rayHits[i].collider.transform;
            if (colliderTransform == null) continue;

            bool isAlreadyHit = false;
            for (int j = 0; j < m_hitTransforms.Count; j++)
            {
                if (m_hitTransforms[j] == colliderTransform)
                {
                    isAlreadyHit = true;
                    break;
                }
            }

            if (isAlreadyHit) continue; 
            m_hitTransforms.Add(colliderTransform);

            // handle players
            if (colliderTransform.parent != null && colliderTransform.parent.HasComponent<NetworkCharacter>())
            {
                colliderTransform.parent.GetComponent<NetworkCharacter>().TakeDamage(10, false);
            }

            // handle destructibles
            if (colliderTransform.HasComponent<Destructible>())
            {
                colliderTransform.GetComponent<Destructible>().TakeDamage(100);
            }
        }
    }
}
