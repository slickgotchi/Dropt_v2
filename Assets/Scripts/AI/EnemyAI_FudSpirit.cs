using Dropt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

namespace Dropt
{
    public class EnemyAI_FudSpirit : EnemyAI
    {
        private Animator m_animator;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public override void OnSpawnStart()
        {
        }

        public override void OnTelegraphStart()
        {
            // calc our attack direction
            CalculateAttackDirection();

            // set our facing direction
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, TelegraphDuration);
        }
        
        public override void OnRoamUpdate(float dt)
        {
            SimpleRoamUpdate(dt);   
        }

        public override void OnAggroUpdate(float dt)
        {
            SimplePursueUpdate(dt);
        }

        public override void OnAttackStart()
        {
            SimpleAttackStart();

            // set facing
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, AttackDuration);
        }

        public override void OnCooldownStart()
        {
            // teleport to new location here
            TeleportToNewAttackPosition();

        }

        public override void OnCooldownUpdate(float dt)
        {
            SimplePursueUpdate(dt);
        }

        public override void OnKnockback(Vector3 direction, float distance, float duration)
        {
            SimpleKnockback(direction, distance, duration);

        }



        // fud spirit teleport
        private void TeleportToNewAttackPosition()
        {
            int maxTeleportAttempts = 10;

            for (int i = 0; i < maxTeleportAttempts; i++)
            {
                var newPosition = Dropt.Utils.Battle.GetRandomSurroundPosition(
                    NearestPlayer.transform.position, 0.8f * AttackRange, AttackRange);

                // check for any overlaps
                // Define the LayerMask using the layers you want to check against
                LayerMask specificLayerMask = LayerMask.GetMask("EnvironmentWall",
                    "EnvironmentWater", "Destructible");
                bool isColliding = Dropt.Utils.Battle.CheckCircleCollision(newPosition, 1f, specificLayerMask);
                if (isColliding)
                {
                    continue;
                } else
                {
                    // teleport to new position
                    transform.position = newPosition;
                    return;
                }
            }
        }
    }
}
