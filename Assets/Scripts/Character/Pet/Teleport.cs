using UnityEngine;
using UnityEngine.AI;

public class Teleport
{
    private readonly NavMeshAgent m_agent;
    private readonly Transform m_target;
    private readonly float m_distance;

    public Teleport(NavMeshAgent navMeshAgent, Transform target, float distance)
    {
        m_agent = navMeshAgent;
        m_target = target;
        m_distance = distance;
    }

    public void Start()
    {
        Vector2 randomPosition;
        bool validPositionFound = false;

        // Attempt to find a valid position on the NavMesh
        for (int i = 0; i < 5; i++)
        {
            // Generate a random point around the target on the X-Y plane
            randomPosition = RandomNavCircle(m_target.position, m_distance);

            // Convert Vector2 to Vector3 for NavMesh.SamplePosition (ignore Z-axis)
            Vector3 randomPosition3D = new(randomPosition.x, randomPosition.y, m_target.position.z);

            // Check if the random position is on the NavMesh
            if (NavMesh.SamplePosition(randomPosition3D, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                // Teleport the agent to the valid NavMesh position
                m_agent.Warp(hit.position);
                validPositionFound = true;
                Debug.Log("Agent teleported to: " + hit.position);
                break;
            }
        }

        if (!validPositionFound)
        {
            Debug.LogWarning("Failed to find a valid position on the NavMesh within the radius.");
        }
    }

    // Generates a random point inside a circle on the X-Y plane
    public Vector2 RandomNavCircle(Vector3 origin, float distance)
    {
        Vector2 randomDirection = Random.insideUnitCircle * distance;

        // Offset the random direction with the origin position
        randomDirection += new Vector2(origin.x, origin.y);

        return randomDirection;
    }
}