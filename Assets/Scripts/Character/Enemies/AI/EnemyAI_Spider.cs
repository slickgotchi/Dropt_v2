using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_Spider : EnemyAI
    {
        private Animator m_animator;

        [HideInInspector] public Vector3 SpawnDirection;
        [HideInInspector] public float SpawnDistance;
        private SoundFX_Spider m_soundFX_Spider;

        private float m_localSpawnTimer = 0f;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            m_soundFX_Spider = GetComponent<SoundFX_Spider>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        public override void OnSpawnStart()
        {
            Utils.Anim.Play(m_animator, "Spider_Jump", interpolationDelay_s);
            base.OnSpawnStart();
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
            Utils.Anim.Play(m_animator, "Spider_Walk", interpolationDelay_s);
        }

        public override void OnRoamUpdate(float dt)
        {
            SimpleRoamUpdate(dt);
        }

        public override void OnAggroStart()
        {
            base.OnAggroStart();

            // walk anim
            Utils.Anim.Play(m_animator, "Spider_Walk", interpolationDelay_s);
        }

        public override void OnAggroUpdate(float dt)
        {
            SimplePursueUpdate(dt);
        }

        public override void OnAttackStart()
        {
            if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
            {
                m_navMeshAgent.isStopped = true;
            }

            SimpleAttackStart();

            // set facing
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, AttackDuration);

            // jump anim
            Utils.Anim.Play(m_animator, "Spider_Jump", interpolationDelay_s);
            m_soundFX_Spider.PlayJumpAttackSound();
        }

        public override void OnAttackFinish()
        {
            base.OnAttackFinish();
        }

        public override void OnCooldownStart()
        {
            if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
            {
                m_navMeshAgent.isStopped = false;
            }
        }

        public override void OnCooldownUpdate(float dt)
        {
            SimpleCooldownUpdate(dt);
        }
    }
}
