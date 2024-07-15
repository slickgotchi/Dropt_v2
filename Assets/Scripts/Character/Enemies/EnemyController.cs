using CarlosLab.UtilityIntelligence;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;


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
    private NavMeshAgent m_navMeshAgent;
    private UtilityAgentController m_utilityAgentController;
    private UtilityAgentFacade m_utilityAgentFacade;

    // spawn parameters
    [Header("Spawn Parameters")]
    [HideInInspector] public bool IsSpawning = true;
    public float SpawnDuration = 0f;

    private void Awake()
    {
        m_localVelocity = GetComponent<LocalVelocity>();
    }

    public override void OnNetworkSpawn()
    {
        // Utility AI
        m_utilityAgentController = GetComponent<UtilityAgentController>();
        m_utilityAgentFacade = GetComponent<UtilityAgentFacade>();

        // only add nav mesh agent on the server
        if (IsServer || IsHost)
        {
            // NavMeshAgent
            m_navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            m_navMeshAgent.updateRotation = false;
            m_navMeshAgent.updateUpAxis = false;
            m_navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            m_navMeshAgent.enabled = true;

            // register with the utility world
            m_utilityAgentController.Register(UtilityWorldSingleton.Instance.World);
        }
        else
        {
            m_utilityAgentController.enabled = false;
            m_utilityAgentFacade.enabled = false;
            return;
        }
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
