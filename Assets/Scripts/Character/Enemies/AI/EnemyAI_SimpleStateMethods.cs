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
            if (m_navMeshAgent == null || !m_navMeshAgent.isOnNavMesh) return;

            m_navMeshAgent.isStopped = false;

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
                m_navMeshAgent.SetDestination(transform.position + finalDirection * distance);
                m_navMeshAgent.speed = networkCharacter.MoveSpeed.Value * RoamSpeedMultiplier;
            }
        }

        // pursue
        protected void SimplePursueUpdate(float dt)
        {
            if (networkCharacter == null) return;
            if (m_navMeshAgent == null || !m_navMeshAgent.isOnNavMesh) return;

            m_navMeshAgent.isStopped = false;

            // get direction from player to enemy and set a small offset
            var dir = (transform.position - NearestPlayer.transform.position).normalized;
            var offset = dir * PursueStopShortRange;

            m_navMeshAgent.SetDestination(NearestPlayer.transform.position + offset);
            m_navMeshAgent.speed = networkCharacter.MoveSpeed.Value * PursueSpeedMultiplier;

            HandleAntiClumping();
            HandleAlertOthers();
        }

        // cooldown - don't move for first half, then do simple pursue
        protected void SimpleCooldownUpdate(float dt)
        {
            if (m_cooldownTimer > 0.5 * CooldownDuration)
            {
                if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
                {
                    m_navMeshAgent.isStopped = true;
                }

            }
            else
            {
                SimplePursueUpdate(dt);
            }

        }

        // flee
        protected void SimpleFleeUpdate(float dt)
        {
            if (networkCharacter == null) return;
            if (m_navMeshAgent == null || !m_navMeshAgent.isOnNavMesh) return;

            m_navMeshAgent.isStopped = false;

            // get direction from player to enemy 
            var dir = (transform.position - NearestPlayer.transform.position).normalized;

            m_navMeshAgent.SetDestination(transform.position + dir * 5f);
            m_navMeshAgent.speed = networkCharacter.MoveSpeed.Value * FleeSpeedMultiplier;

            HandleAntiClumping();
            HandleAlertOthers();
        }

        protected void HandleAntiClumping()
        {
            if (networkCharacter == null) return;
            if (m_navMeshAgent == null) return;

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
            Vector3 finalDestination = m_navMeshAgent.destination + avoidanceForce;

            // Set the adjusted destination
            m_navMeshAgent.SetDestination(finalDestination);
        }

        protected void HandleAlertOthers()
        {
            var allEnemies = EnemyAIManager.Instance.allEnemies;
            for (int i = 0; i < allEnemies.Count; i++)
            {
                var otherEnemy = allEnemies[i];
                if (otherEnemy.state.Value != State.Roam) continue;

                var dist = math.distance(transform.position, otherEnemy.transform.position);
                if (dist < AlertRange)
                {
                    otherEnemy.state.Value = State.Aggro;
                }
            }
        }

        // telegraph
        private void CalculateAttackDirectionAndPosition()
        {
            // get target attack centre
            var targetCentre = NearestPlayer.GetComponentInChildren<AttackCentre>();
            var targetCentrePos = (targetCentre == null) ? NearestPlayer.transform.position : targetCentre.transform.position;

            // get our attack centre
            var enemyCentre = GetComponentInChildren<AttackCentre>();
            var enemyCentrePos = (enemyCentre == null) ? transform.position : enemyCentre.transform.position;

            // set attack direction
            AttackDirection = (targetCentrePos - enemyCentrePos).normalized;

            // set attack position
            PositionToAttack = targetCentrePos;

            //return AttackDirection;
        }

        // attack
        protected void SimpleAttackStart()
        {
            // check we have a primary attack.
            if (PrimaryAttack == null) return;
            if (m_navMeshAgent == null || !m_navMeshAgent.isOnNavMesh) return;

            // stop navmeshagent
            m_navMeshAgent.isStopped = true;

            // instantiate an attack
            GameObject ability = Instantiate(PrimaryAttack, transform.position, Quaternion.identity);

            // get enemy ability of attack
            EnemyAbility enemyAbility = ability.GetComponent<EnemyAbility>();
            if (enemyAbility == null) return;

            // initialise the ability
            ability.GetComponent<NetworkObject>().Spawn();
            enemyAbility.Activate(gameObject, NearestPlayer, AttackDirection, AttackDuration, PositionToAttack);

            // ensure state is attack
            state.Value = State.Attack;
        }
    }
}
