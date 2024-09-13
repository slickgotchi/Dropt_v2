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

        protected void SimpleRoamUpdate(float dt)
        {
            if (networkCharacter == null) return;
            if (navMeshAgent == null) return;

            navMeshAgent.isStopped = false;

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
                float distance = networkCharacter.MoveSpeed.Value * RoamSpeedMultiplier * m_roamChangeTimer;

                // set nav mesh agent
                navMeshAgent.SetDestination(transform.position + finalDirection * distance);
                navMeshAgent.speed = networkCharacter.MoveSpeed.Value * RoamSpeedMultiplier;
            }
        }

        // pursue
        protected void SimplePursueUpdate(float dt)
        {
            if (networkCharacter == null) return;
            if (navMeshAgent == null) return;

            navMeshAgent.isStopped = false;

            // get direction from player to enemy and set a small offset
            var dir = (transform.position - NearestPlayer.transform.position).normalized;
            var offset = dir * AttackRange * 0.9f;

            navMeshAgent.SetDestination(NearestPlayer.transform.position + offset);
            navMeshAgent.speed = networkCharacter.MoveSpeed.Value * PursueSpeedMultiplier;

            HandleAntiClumping();
            HandleAlertOthers();
        }

        private void HandleAntiClumping()
        {
            if (networkCharacter == null) return;
            if (navMeshAgent == null) return;

            var allEnemies = EnemyAIManager.Instance.allEnemies;
            //var minDistance = 1.5f;
            var repellingForce = 5f;

            Vector3 avoidanceForce = Vector3.zero; // Initialize the avoidance force

            // Check all other enemies
            int numEnemies = allEnemies.Count;
            for (int i = 0; i < numEnemies; i++)
            {
                var otherEnemy = allEnemies[i];
                if (otherEnemy == this) continue; // Skip itself
                if (otherEnemy == null) continue;
                if (!otherEnemy.gameObject.activeInHierarchy) continue;

                // Check distance to the other enemy
                float distance = math.distance(transform.position, otherEnemy.transform.position);
                float minDistance = avoidanceRadius + otherEnemy.avoidanceRadius;

                // If within the clumping radius, apply a repelling force
                if (distance < minDistance && distance > 0.1f)
                {
                    Vector3 directionAway = (transform.position - otherEnemy.transform.position).normalized;
                    float repellingStrength = (minDistance - distance) / minDistance; // Scale the repelling force
                    avoidanceForce += directionAway * repellingStrength * repellingForce;
                }
            }

            // Adjust the agent's next destination slightly based on avoidance force
            Vector3 finalDestination = navMeshAgent.destination + avoidanceForce;

            // Set the adjusted destination
            navMeshAgent.SetDestination(finalDestination);
        }

        private void HandleAlertOthers()
        {
            var allEnemies = EnemyAIManager.Instance.allEnemies;
            for (int i = 0; i < allEnemies.Count; i++)
            {
                var otherEnemy = allEnemies[i];
                if (otherEnemy.state != State.Roam) continue;

                var dist = math.distance(transform.position, otherEnemy.transform.position);
                if (dist < AlertRange)
                {
                    otherEnemy.state = State.Aggro;
                }
            }
        }

        // telegraph
        public Vector3 CalculateAttackDirection()
        {
            // get target attack centre
            var targetCentre = NearestPlayer.GetComponentInChildren<AttackCentre>();
            var targetCentrePos = (targetCentre == null) ? NearestPlayer.transform.position : targetCentre.transform.position;

            // get our attack centre
            var enemyCentre = GetComponentInChildren<AttackCentre>();
            var enemyCentrePos = (enemyCentre == null) ? transform.position : enemyCentre.transform.position;

            // set attack direction
            AttackDirection = (targetCentrePos - enemyCentrePos).normalized;

            return AttackDirection;
        }

        // attack
        protected void SimpleAttackStart()
        {
            // check we have a primary attack.
            if (PrimaryAttack == null) return;

            // instantiate an attack
            var ability = Instantiate(PrimaryAttack);
            
            // get enemy ability of attack
            var enemyAbility = ability.GetComponent<EnemyAbility>();
            if (enemyAbility == null) return;

            // initialise the ability
            ability.GetComponent<NetworkObject>().Spawn();
            enemyAbility.Init(gameObject, NearestPlayer);
            enemyAbility.Activate();
            
        }

        // knockback
        protected void SimpleKnockback(Vector3 direction, float distance, float duration)
        {
            if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.gameObject.activeInHierarchy) return;

            // NEEDS TO BE RAY OR COLLIDER CASTED!!!
            m_stunTimer = duration;
            transform.position += direction.normalized * distance;

            navMeshAgent.isStopped = true;
        }
    }
}
