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

        [HideInInspector] public Vector3 SpawnDirection;
        [HideInInspector] public float SpawnDistance;

        private float m_localSpawnTimer = 0f;


        private float m_interpDelay = 0.3f;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        public override void OnSpawnStart()
        {
            m_interpDelay = IsHost ? 0 : 3 * 1 / (float)NetworkManager.Singleton.NetworkTickSystem.TickRate;

            m_animator.Play("Spider_Jump");
        }

        public override void OnSpawnUpdate(float dt)
        {
            base.OnSpawnUpdate(dt);

            m_localSpawnTimer += dt;
            if (m_localSpawnTimer > (0.4f / 0.6f) * SpawnDuration) return;   // this is the jumping part of the anim and ensures we don't move when spider lands

            var speed = (SpawnDistance / SpawnDuration) / (0.4f / 0.6f);
            transform.position += SpawnDirection * speed * dt;
        }

        public override void OnSpawnFinish()
        {
            base.OnSpawnFinish();
        }

        public override void OnTelegraphStart()
        {
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
            GetComponent<NavMeshAgent>().isStopped = true;

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
            GetComponent<NavMeshAgent>().isStopped = false;
        }

        public override void OnCooldownUpdate(float dt)
        {
            SimplePursueUpdate(dt);

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
        }

    }
}
