using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CarlosLab.UtilityIntelligence;
using CarlosLab.Common;
using UnityEngine.AI;

public class ActionTask_Flee : ActionTask
{
    public VariableReference<float> FleeSpeed = 5.0f;
    public VariableReference<float> FleeDistance = 10f;
    public float VisionArc = 180f; // Arc of vision in degrees
    public float LineOfSightDistance = 10f; // Length of the raycasts
    public int NumberOfRays = 10; // Number of raycasts to perform
    public float StartRayLineOffset = 1f; // Offset for starting the raycasts

    protected override void OnAwake()
    {
    }

    protected override UpdateStatus OnUpdate(float deltaTime)
    {
        // ensure we still have a valid context and target
        if (Context == null || Context.Target == null) return UpdateStatus.Running;

        // Note: this should only run server side (because we remove NavMeshAgent from our client side players)
        var navMeshAgent = GameObject.GetComponent<NavMeshAgent>();
        if (navMeshAgent == null) return UpdateStatus.Running;
        navMeshAgent.isStopped = false;

        // try get target
        var target = Context.Target.GetComponent<Transform>();
        if (target == null) return UpdateStatus.Running;

        // Find the best direction to flee
        Vector3 bestDirection = FindBestFleeDirection(navMeshAgent.transform, target.position);

        // Set the destination based on the best direction
        navMeshAgent.SetDestination(navMeshAgent.transform.position + bestDirection * FleeDistance);
        navMeshAgent.speed = FleeSpeed;

        return UpdateStatus.Running;
    }

    private Vector3 FindBestFleeDirection(Transform self, Vector3 targetPosition)
    {
        Vector3 bestDirection = Vector3.zero;
        float maxDistance = 0f;
        float maxTargetDistance = float.MinValue; // Start with the worst possible value

        // Calculate the initial direction (away from the target)
        Vector3 initialDirection = (self.position - targetPosition).normalized;

        // Calculate the angle step based on the number of rays and the vision arc
        float angleStep = VisionArc / (NumberOfRays - 1);
        float halfArc = VisionArc / 2;

        // Iterate over the number of rays
        for (int i = 0; i < NumberOfRays; i++)
        {
            // Calculate the current angle
            float angle = -halfArc + i * angleStep;

            // Calculate the direction for this ray
            Vector3 direction = Quaternion.Euler(0, 0, angle) * initialDirection;

            // Calculate the starting point of the ray with the offset
            Vector3 rayStartPosition = self.position + direction * StartRayLineOffset;

            // Perform the raycast
            RaycastHit2D hit = Physics2D.Raycast(rayStartPosition, direction, LineOfSightDistance, LayerMask.GetMask("EnvironmentWall", "EnvironmentWater", "Destructible"));

            // Determine the distance of the raycast (either the hit point or the max distance)
            float distance = hit.collider != null ? hit.distance : LineOfSightDistance;

            // Calculate the distance to the target if we were to move in this direction
            float targetDistance = Vector3.Distance(rayStartPosition + direction * distance, targetPosition);

            // Check if this direction is the best one so far
            // Prioritize the direction that maximizes distance from obstacles, but if all directions are closer to the target,
            // choose the direction that moves away from the target the most
            if (distance > maxDistance || (distance == maxDistance && targetDistance > maxTargetDistance))
            {
                maxDistance = distance;
                maxTargetDistance = targetDistance;
                bestDirection = direction;
            }
        }

        // Return the best direction found (normalized)
        return bestDirection.normalized;
    }


}
