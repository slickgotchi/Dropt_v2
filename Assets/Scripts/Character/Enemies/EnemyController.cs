using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

public class EnemyController : NetworkBehaviour
{
    [Header("Rendering Parameters")]
    [SerializeField] private List<SpriteRenderer> m_spritesToFlip;
    public enum Facing { Left, Right }
    private Facing m_facingDirection;

    private NavMeshAgent m_navMeshAgent;

    // private parameters for updating facing position
    private float m_facingTimer = 0f;

    // spawn parameters
    [Header("Spawn Parameters")]
    [HideInInspector] public bool IsSpawning = true;
    public float SpawnDuration = 0f;

    //private Vector3 m_agentVelocity;
    private Vector3 m_currentPosition;
    private Vector3 m_previousPosition;
    private float m_previousPositionUpdateTimer = 0f;

    //[HideInInspector] public bool IsArmed = false;

    public Dropt.EnemyAI enemyAI;
    private ProximityCulling m_proximityCulling;

    private bool isServer = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Game.Instance.enemyControllers.Add(this);

        isServer = Bootstrap.IsServer();

        enemyAI = GetComponent<Dropt.EnemyAI>();
        m_proximityCulling = GetComponent<ProximityCulling>();

        if (IsClient && !IsHost)
        {
            Destroy(GetComponent<NavMeshAgent>());
        }
        else
        {
            m_navMeshAgent = GetComponent<NavMeshAgent>();
            if (m_navMeshAgent == null) return;

            m_navMeshAgent.updateRotation = false;
            m_navMeshAgent.updateUpAxis = false;
            m_navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            m_navMeshAgent.enabled = true;
        }

    }

    public override void OnNetworkDespawn()
    {
        Game.Instance.enemyControllers.Remove(this);

        // remove any level spawn components
        LevelSpawnManager.Instance.RemoveLevelSpawnComponent(GetComponent<Level.LevelSpawn>());

        base.OnNetworkDespawn();
    }

    // Update is called once per frame
    void Update()
    {
        // update previous transform once every 0.1s
        m_previousPositionUpdateTimer -= Time.deltaTime;
        if (m_previousPositionUpdateTimer < 0)
        {
            m_previousPositionUpdateTimer = 0.1f;
            m_previousPosition = m_currentPosition;
            m_currentPosition = transform.position;

            HandleFacing();
        }

    }

    public void SetFacingFromDirection(Vector3 direction, float facingTimer)
    {
        // client or host
        if (IsHost || IsServer)
        {
            SetFacingFromDirectionClientRpc(direction, facingTimer);
        }
    }

    [ClientRpc]
    void SetFacingFromDirectionClientRpc(Vector3 direction, float facingTimer)
    {
        SetFacing(direction.x > 0 ? Facing.Right : Facing.Left, facingTimer);
    }

    public void SetFacing(Facing facingDirection, float facingTimer = 0.5f)
    {
        m_facingTimer = facingTimer;
        m_facingDirection = facingDirection;
        SetEnemySpritesFlip();
    }


    public void HandleFacing()
    {
        if (isServer) return;

        if (m_spritesToFlip.Count == 0) return;

        if (IsClient)
        {
            // if in knockback/stun states we don't flip sprites
            if (enemyAI != null)
            {
                if (enemyAI.GetClientPredictedState() == Dropt.EnemyAI.State.Knockback) return;
                if (enemyAI.GetClientPredictedState() == Dropt.EnemyAI.State.Stun) return;
            }

            // if there is active facing timer we don't flip sprites
            m_facingTimer -= Time.deltaTime;
            if (m_facingTimer > 0f) return;

            // find our position delta (use 0.01f, if we update positions every 0.1s this equates to a 
            var positionDeltaX = m_currentPosition.x - m_previousPosition.x;
            if (positionDeltaX > 0.01f)
            {
                m_facingDirection = Facing.Right;
            }
            else if (positionDeltaX < -0.01f)
            {
                m_facingDirection = Facing.Left;
            }

            // set the sprite flip
            SetEnemySpritesFlip();
        }
    }

    private void SetEnemySpritesFlip()
    {
        foreach (SpriteRenderer spriteRenderer in m_spritesToFlip)
        {
            spriteRenderer.flipX = m_facingDirection == Facing.Left;
        }
    }
}
