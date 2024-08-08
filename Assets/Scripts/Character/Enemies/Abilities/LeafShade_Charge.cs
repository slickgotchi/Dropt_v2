using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LeafShade_Charge : EnemyAbility
{
    [Header("LeafShade_Charge Parameters")]
    public float ChargeDistance = 4f;

    private Vector3 m_direction;
    private float m_speed;
    //private Collider2D m_collider;
    private bool m_isExecuting = false;

    private List<Transform> m_hitTransforms = new List<Transform>();

    private RaycastHit2D[] m_wallHits = new RaycastHit2D[1];
    private RaycastHit2D[] m_objectHits = new RaycastHit2D[10];

    [SerializeField] private Collider2D m_damageCollider;
    [SerializeField] private Collider2D m_moveCollider;

    private void Awake()
    {
    }

    public override void OnNetworkSpawn()
    {
    }

    public override void OnTelegraphStart()
    {
        transform.position = Parent.transform.position;
        m_direction = AttackDirection;
        m_speed = ChargeDistance / ExecutionDuration;

        EnemyController.Facing facing = m_direction.x > 0 ? EnemyController.Facing.Right : EnemyController.Facing.Left;
        if (Parent != null) Parent.GetComponent<EnemyController>().SetFacingDirection(facing);
    }

    public override void OnExecutionStart()
    {
        m_isExecuting = true;
    }

    public override void OnCooldownStart()
    {
        m_isExecuting = false;
    }

    public override void OnUpdate()
    {
        if (!m_isExecuting) return;

        HandleCharge();
    }

    public void HandleCharge()
    {
        // 1. sync transoforms
        Physics2D.SyncTransforms();

        // 2. determine how far we can move (check for wall/water collisions)
        Vector2 castDirection = m_direction;
        float castDistance = m_speed * Time.deltaTime;
        int hitCount = m_moveCollider.Cast(castDirection,
            PlayerAbility.GetContactFilter(new string[] { "EnvironmentWall", "EnvironmentWater" }),
            m_wallHits, castDistance);

        if (hitCount > 0)
        {
            var rayHit = m_wallHits[0];
            castDistance = rayHit.distance;
        }

        // 3. perform collisions using the new (if applicable) cast distance
        m_damageCollider.Cast(castDirection,
            PlayerAbility.GetContactFilter(new string[] { "PlayerHurt", "Destructible" }),
            m_objectHits, castDistance);

        // 4. iterate over any object  hits
        for (int i = 0; i < m_objectHits.Length; i++)
        {
            var collider = m_objectHits[i].collider;
            if (collider == null) continue;
            var colliderTransform = collider.transform;
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

        transform.position += m_direction * castDistance;
        if (Parent != null) Parent.transform.position = transform.position;
    }
}
