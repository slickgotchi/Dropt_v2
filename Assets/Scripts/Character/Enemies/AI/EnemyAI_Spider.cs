using Dropt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_Spider : EnemyAI
    {
        private Animator m_animator;

        private float m_interpDelay = 0.3f;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public override void OnSpawnStart()
        {
            m_interpDelay = IsHost ? 0 : 3 * 1 / (float)NetworkManager.Singleton.NetworkTickSystem.TickRate;

        }

        public override void OnTelegraphStart()
        {
            // calc our attack direction
            CalculateAttackDirectionAndPosition();

            // set our facing direction
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, TelegraphDuration);
        }

        public override void OnRoamStart()
        {
            base.OnRoamStart();

            // walk anim
            Invoke("PlayWalkAnimation", m_interpDelay);
        }

        public override void OnRoamUpdate(float dt)
        {
            SimpleRoamUpdate(dt);   
        }

        public override void OnAggroStart()
        {
            base.OnAggroStart();

            // walk anim
            Invoke("PlayWalkAnimation", m_interpDelay);
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

            // jump anim
            Invoke("PlayJumpAnimation", m_interpDelay);
        }

        public override void OnAttackFinish()
        {
            base.OnAttackFinish();

            Invoke("SpawnStompCircle", m_interpDelay);
        }

        public override void OnCooldownStart()
        {
            GetComponent<NavMeshAgent>().isStopped = true;
        }

        public override void OnCooldownUpdate(float dt)
        {
            //SimplePursueUpdate(dt);
            
        }

        public override void OnKnockback(Vector3 direction, float distance, float duration)
        {
            SimpleKnockback(direction, distance, duration);
        }



        void PlayJumpAnimation()
        {
            GetComponent<Animator>().Play("Spider_Jump");
        }

        void PlayWalkAnimation()
        {
            GetComponent<Animator>().Play("Spider_Walk");
        }

        void SpawnStompCircle()
        {
            //SpawnBasicCircleClientRpc(
            //    transform.position,
            //    Dropt.Utils.Color.HexToColor("#622461", 0.5f),
            //    1f);
        }

    }
}
