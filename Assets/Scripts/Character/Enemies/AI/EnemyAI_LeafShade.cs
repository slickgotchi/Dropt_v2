using UnityEngine;

namespace Dropt
{
    public class EnemyAI_LeafShade : EnemyAI
    {
        private Animator m_animator;
        private SoundFX_Shade m_soundFX_Shade;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            m_soundFX_Shade = GetComponent<SoundFX_Shade>();
        }

        public override void OnSpawnStart()
        {
            base.OnSpawnStart();

            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "LeafShade_Spawn", SpawnDuration);
            }
        }

        public override void OnTelegraphStart()
        {
            if (IsServer)
            {
                // stop nav mesh
                if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
                {
                    m_navMeshAgent.isStopped = true;
                }

                Utils.Anim.PlayAnimationWithDuration(m_animator, "LeafShade_Anticipation", TelegraphDuration, interpolationDelay_s);

            }
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, 0.1f);
            m_soundFX_Shade.PlayChargeSound();
        }

        public override void OnRoamStart()
        {
            base.OnRoamStart();
            if (IsServer)
            {
                Utils.Anim.Play(m_animator, "LeafShade_Roam", interpolationDelay_s);
            }
        }

        public override void OnRoamUpdate(float dt)
        {
            SimpleRoamUpdate(dt);
        }

        public override void OnAggroStart()
        {
            base.OnAggroStart();
            if (IsServer)
            {
                Utils.Anim.Play(m_animator, "LeafShade_Roam", interpolationDelay_s);
            }
        }

        public override void OnAggroUpdate(float dt)
        {
            SimplePursueUpdate(dt);
        }

        public override void OnAttackStart()
        {
            if (IsServer)
            {
                Utils.Anim.Play(m_animator, "LeafShade_Attack", interpolationDelay_s);
            }
            SimpleAttackStart();
            m_soundFX_Shade.PlayAttackSound();
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, 0.1f);
        }

        public override void OnCooldownStart()
        {
            if (IsServer)
            {
                Utils.Anim.Play(m_animator, "LeafShade_Roam", interpolationDelay_s);
            }
        }

        public override void OnCooldownUpdate(float dt)
        {
            SimpleCooldownUpdate(dt);
        }
    }
}