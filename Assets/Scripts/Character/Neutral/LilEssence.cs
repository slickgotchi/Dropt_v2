using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

public class LilEssence : NetworkBehaviour
{
    private Animator m_animator;
    private NavMeshAgent m_navmeshAgent;

    public enum EssenceState { Roam, Flee, Caught, Dead }
    public EssenceState state = EssenceState.Roam;

    public float RoamSpeed = 2f;
    public float FleeSpeed = 7f;
    public float FleeRadius = 5f;
    public float BreakFleeRadius = 10f;
    public float EssenceReward = 10f;
    public float WanderRadius = 3f; // Radius around current position for wandering
    public float WanderInterval = 3f; // Time in seconds between wander destinations
    private float wanderTimer; // Tracks time until next wander destination
    private Vector3 currentWanderTarget;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_navmeshAgent = GetComponent<NavMeshAgent>();

        // Ensure NavMeshAgent is constrained to the XY plane
        m_navmeshAgent.updateUpAxis = false;
        m_navmeshAgent.updateRotation = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient && !IsHost)
        {
            Destroy(m_navmeshAgent);
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        switch (state)
        {
            case EssenceState.Roam:
                HandleRoam(dt);
                break;
            case EssenceState.Flee:
                HandleFlee(dt);
                break;
            case EssenceState.Caught:
                HandleCaught(dt);
                break;
            case EssenceState.Dead:
                break;
            default:
                break;
        }
    }

    void HandleRoam(float dt)
    {
        if (!IsServer) return;

        if (m_navmeshAgent != null)
        {
            // Check if there are any players within the flee radius to start fleeing
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                var dist = math.distance(transform.position, player.transform.position);
                if (dist < FleeRadius)
                {
                    state = EssenceState.Flee;
                    return;
                }
            }

            m_navmeshAgent.speed = RoamSpeed;

            // Wander around if no players are close
            wanderTimer += dt;
            if (wanderTimer >= WanderInterval || m_navmeshAgent.remainingDistance < 0.5f)
            {
                SetNewWanderTarget();
                wanderTimer = 0f; // Reset the timer
            }
        }
    }

    private void SetNewWanderTarget()
    {
        // Generate a random direction and distance for the new wander target within the XY plane
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(1f, WanderRadius);
        Vector3 wanderTarget = new Vector3(transform.position.x + randomDirection.x, transform.position.y + randomDirection.y, 0);

        // Set the NavMeshAgent destination to the new wander target
        if (NavMesh.SamplePosition(wanderTarget, out NavMeshHit hit, WanderRadius, NavMesh.AllAreas))
        {
            currentWanderTarget = hit.position;
            m_navmeshAgent.SetDestination(new Vector3(currentWanderTarget.x, currentWanderTarget.y, 0));
        }
    }

    void HandleFlee(float dt)
    {
        if (!IsServer) return;

        if (m_navmeshAgent != null)
        {
            var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            var closestDist = 100f;
            PlayerController closestPlayer = null;
            foreach (var player in players)
            {
                var dist = math.distance(transform.position, player.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestPlayer = player;
                }
            }

            if (closestDist > BreakFleeRadius)
            {
                state = EssenceState.Roam;
                return;
            }

            // Calculate flee direction and add randomness in the XY plane
            var fleeVector = (transform.position - closestPlayer.transform.position).normalized;
            var randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0).normalized;
            var erraticFleeDirection = (fleeVector + randomDirection * 0.5f).normalized;

            // Calculate a random target position around the flee direction within the XY plane
            var targetFleePosition = closestPlayer.transform.position + erraticFleeDirection * UnityEngine.Random.Range(BreakFleeRadius * 1.1f, BreakFleeRadius * 1.5f);

            // Set NavMesh destination and add slight random speed variation for erratic effect, with position constrained to XY
            m_navmeshAgent.SetDestination(new Vector3(targetFleePosition.x, targetFleePosition.y, 0));
            m_navmeshAgent.speed = FleeSpeed;
        }
    }

    void HandleCaught(float dt)
    {
        if (!IsServer) return;

        state = EssenceState.Dead;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        PlayerOffchainData playerData = collision.gameObject.GetComponent<PlayerOffchainData>();
        if (playerData != null)
        {
            playerData.AddEssence(EssenceReward);
            gameObject.GetComponent<NetworkObject>().Despawn();

            state = EssenceState.Caught;
        }
    }
}
