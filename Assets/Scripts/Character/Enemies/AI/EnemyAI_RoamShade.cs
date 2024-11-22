using Dropt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_RoamShade : EnemyAI
    {
        private Animator m_animator;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public override void OnSpawnStart()
        {
            base.OnSpawnStart();
        }

        public override void OnTelegraphStart()
        {
        }

        public override void OnRoamUpdate(float dt)
        {
            Patrol(dt);
        }

        public override void OnAggroUpdate(float dt)
        {
            Patrol(dt);
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

        bool isGoToA = false;

        Vector3 a = new Vector3(-5, 15, 0);
        Vector3 b = new Vector3(-5, -15, 0);

        void Patrol(float dt)
        {
            if (networkCharacter == null) return;
            if (m_navMeshAgent == null) return;

            m_navMeshAgent.isStopped = false;
            m_navMeshAgent.speed = 10;

            if (isGoToA)
            {
                m_navMeshAgent.SetDestination(a);
                var dist = math.distance(a, transform.position);
                if (dist < 0.5f) isGoToA = false;
            }
            else
            {
                m_navMeshAgent.SetDestination(b);
                var dist = math.distance(b, transform.position);
                if (dist < 0.5f) isGoToA = true;
            }

        }
    }
}
