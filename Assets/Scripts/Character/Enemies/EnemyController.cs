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
    public Facing FacingDirection;

    // private parameters for updating facing position
    private LocalVelocity m_localVelocity;
    private float m_facingTimer = 0f;

    // AI parameters
    //private NavMeshAgent m_navMeshAgent;

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
        Debug.Log("Spawn Enemy");
        // Utility AI

        if (IsClient && !IsHost)
        {
            Debug.Log("Remove NavMeshAgent from client side enemy");
            Destroy(GetComponent<NavMeshAgent>());
        } else
        {
            var navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;
            navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            navMeshAgent.enabled = true;
        }

        //// only add nav mesh agent on the server
        //if (IsServer || IsHost)
        //{
        //    // NavMeshAgent
        //    m_navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        //    m_navMeshAgent.updateRotation = false;
        //    m_navMeshAgent.updateUpAxis = false;
        //    m_navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        //    m_navMeshAgent.enabled = true;

        //    // register with the utility world
        //}
        //else
        //{
        //    return;
        //}
    }

    public override void OnNetworkDespawn()
    {
        //if (m_utilityAgentFacade != null) m_utilityAgentFacade.Destroy();
    }

    // Update is called once per frame
    void Update()
    {
        HandleFacing();

        SpawnDuration -= Time.deltaTime;

        if (SpawnDuration <= 0f)
        {
            IsSpawning = false;
        }
    }

    public void SetFacingDirection(Facing facingDirection, float facingTimer = 0.5f)
    {
        m_facingTimer = facingTimer;
        FacingDirection = facingDirection;
    }

    public void HandleFacing()
    {
        if (SpriteToFlip == null) return;

        m_facingTimer -= Time.deltaTime;

        if (m_facingTimer > 0f)
        {
            SpriteToFlip.flipX = FacingDirection == Facing.Left;
        }
        else if (m_localVelocity.IsMoving)
        {
            SpriteToFlip.flipX = m_localVelocity.Value.x > 0 ? false : true;
        }
    }
}
