using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Mathematics;
using UnityEngine.AI;

// SimpleMethods - useful AI methods to use in derived classes
namespace Dropt
{
    public partial class EnemyAI : NetworkBehaviour
    {
        // roam
        private float m_roamChangeTimer = 0f;
        private float m_roamChangeInterval = 2.25f;
        private float m_roamChangeIntervalVariance = 0.75f;

        protected void SimpleRoam(float dt)
        {
            if (NetworkCharacter == null) return;
            if (NavMeshAgent == null) return;

            m_roamChangeTimer -= dt;
            if (m_roamChangeTimer < 0f)
            {
                // reset the timer till we need to change direction again
                m_roamChangeTimer = UnityEngine.Random.Range(m_roamChangeInterval - m_roamChangeIntervalVariance, m_roamChangeInterval + m_roamChangeIntervalVariance);

                // get a random direction & the direction back to our anchor point
                Vector3 randomDirection = (new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0)).normalized;
                Vector3 toAnchorDirection = (RoamAnchorPoint - transform.position).normalized;

                // calc influence factor
                float distanceToAnchor = math.distance(transform.position, RoamAnchorPoint);
                float influenceFactor = math.min(distanceToAnchor / MaxRoamRange, 1f);

                Vector3 finalDirection = math.lerp(randomDirection, toAnchorDirection, influenceFactor);

                // calc a move distance
                float distance = NetworkCharacter.MoveSpeed.Value * RoamSpeedMultiplier * m_roamChangeTimer;

                // set nav mesh agent
                NavMeshAgent.SetDestination(transform.position + finalDirection * distance);
                NavMeshAgent.speed = NetworkCharacter.MoveSpeed.Value * RoamSpeedMultiplier;
            }
        }

        // pursue
        protected void SimplePursue(float dt)
        {
            if (NetworkCharacter == null) return;
            if (NavMeshAgent == null) return;

            // get direction from player to enemy and set a small offset
            var dir = (transform.position - NearestPlayer.transform.position).normalized;
            var offset = dir * 0.5f;

            NavMeshAgent.SetDestination(NearestPlayer.transform.position + offset);
            NavMeshAgent.speed = NetworkCharacter.MoveSpeed.Value * PursueSpeedMultiplier;
        }
    }
}
