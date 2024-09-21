using Dropt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_LeafShade : EnemyAI
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
        }

        public override void OnCooldownUpdate(float dt)
        {
            SimplePursueUpdate(dt);
        }
    }
}
