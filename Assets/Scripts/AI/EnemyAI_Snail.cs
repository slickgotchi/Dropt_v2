using Dropt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

namespace Dropt
{
    public class EnemyAI_Snail : EnemyAI
    {
        private Animator m_animator;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public override void OnSpawnStart()
        {
            // play anim
            Dropt.Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Unburrow", SpawnDuration);
        }

        public override void OnTelegraphStart()
        {
            // play anim
            Dropt.Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_TelegraphAttack", TelegraphDuration);

            // calc our attack direction
            CalculateAttackDirection();

            // set our facing direction
            EnemyController.Facing facing = AttackDirection.x > 0 ? EnemyController.Facing.Right : EnemyController.Facing.Left;
            GetComponent<EnemyController>().SetFacingDirection(facing, 1f);
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
        }

        public override void OnKnockbackStart(Vector3 direction, float distance, float duration)
        {
            SimpleKnockback(direction, distance, duration);

            // stop animator
            m_animator.Play("Snail_Idle");
        }
    }
}
