using Dropt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

namespace Dropt
{
    public class EnemyAI_SentryBot : EnemyAI
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
        }
        
        public override void OnRoamUpdate(float dt)
        {
            SimpleRoamUpdate(dt);   
        }

        public override void OnFleeUpdate(float dt)
        {
            base.OnFleeUpdate(dt);

            SimpleFleeUpdate(dt);
        }

        public override void OnAggroUpdate(float dt)
        {
            SimpleFleeUpdate(dt);
        }

        public override void OnAttackStart()
        {
        }

        public override void OnCooldownStart()
        {
        }

        public override void OnCooldownUpdate(float dt)
        {
        }

        //public override void OnKnockback(Vector3 direction, float distance, float duration)
        //{
        //    SimpleKnockback(direction, distance, duration);
        //}
    }
}
