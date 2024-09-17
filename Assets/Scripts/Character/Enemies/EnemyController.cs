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

    private NetworkVariable<Vector3> m_agentVelocity = new NetworkVariable<Vector3>();

    [HideInInspector] public bool IsArmed = false;

    private void Awake()
    {
        m_localVelocity = GetComponent<LocalVelocity>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsHost)
        {
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
        // client or host
        if (IsClient || IsHost)
        {
            SetFacing(direction.x > 0 ? Facing.Right : Facing.Left, facingTimer);
        }
        // server
        else
        {
            SetFacingFromDirectionClientRpc(direction, facingTimer);
        }
    }

    [ClientRpc]
    void SetFacingFromDirectionClientRpc(Vector3 direction, float facingTimer)
    {
        SetFacingFromDirection(direction, facingTimer);
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
        

        if (IsServer)
        {
            m_agentVelocity.Value = m_navMeshAgent != null ? m_navMeshAgent.velocity : Vector3.zero;
        }

        if (IsClient)
        {
            if (math.abs(m_agentVelocity.Value.x) < 0.02f) return;

            m_facingTimer -= Time.deltaTime;
            if (m_facingTimer > 0f) return;

            FacingDirection = m_agentVelocity.Value.x < 0 ? Facing.Left : Facing.Right;
            SpriteToFlip.flipX = FacingDirection == Facing.Left ? true : false;
        }
    }
}
