using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

public class EnemyController : NetworkBehaviour
{
    [Header("Rendering Parameters")]
    public SpriteRenderer SpriteToFlip;
    public enum Facing { Left, Right }
    [HideInInspector] public Facing FacingDirection;

    private NavMeshAgent m_navMeshAgent;

    // private parameters for updating facing position
    private LocalVelocity m_localVelocity;
    private float m_facingTimer = 0f;

    // spawn parameters
    [Header("Spawn Parameters")]
    [HideInInspector] public bool IsSpawning = true;
    public float SpawnDuration = 0f;

    [HideInInspector] public bool IsArmed = false;

    private void Awake()
    {
        m_localVelocity = GetComponent<LocalVelocity>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsHost)
        {
            Debug.Log("Remove NavMeshAgent from client side enemy");
            Destroy(GetComponent<NavMeshAgent>());
        } else
        {
            m_navMeshAgent = GetComponent<NavMeshAgent>();
            if (m_navMeshAgent == null) gameObject.AddComponent<NavMeshAgent>();

            m_navMeshAgent.updateRotation = false;
            m_navMeshAgent.updateUpAxis = false;
            m_navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            m_navMeshAgent.enabled = true;

        }

    }

    public override void OnNetworkDespawn()
    {
    }

    // Update is called once per frame
    void Update()
    {
        HandleFacing();
    }

    public void SetFacingFromDirection(Vector3 direction, float facingTimer)
    {
        SetFacing(direction.x > 0 ? Facing.Right : Facing.Left, facingTimer);
    }

    public void SetFacing(Facing facingDirection, float facingTimer = 0.5f)
    {
        m_facingTimer = facingTimer;
        FacingDirection = facingDirection;
        SpriteToFlip.flipX = FacingDirection == Facing.Left ? true : false;
    }

    public void HandleFacing()
    {
        if (SpriteToFlip == null) return;
        if (m_navMeshAgent == null) return;
        if (math.abs(m_navMeshAgent.velocity.x) < 0.02f) return;

        m_facingTimer -= Time.deltaTime;
        if (m_facingTimer > 0f) return;

        FacingDirection = m_navMeshAgent.velocity.x < 0 ? Facing.Left : Facing.Right;
        SpriteToFlip.flipX = FacingDirection == Facing.Left ? true : false;
    }
}
