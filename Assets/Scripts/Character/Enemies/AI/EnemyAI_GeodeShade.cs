using UnityEngine;

namespace Dropt
{
    public class EnemyAI_GeodeShade : EnemyAI
    {
        private Animator m_animator;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public override void OnSpawnStart()
        {
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator,
                                                     "GeodeShade_Spawn",
                                                     SpawnDuration);
            }
        }

        public override void OnTelegraphStart()
        {
            if (IsServer)
            {
                m_navMeshAgent.isStopped = true;
                Utils.Anim.PlayAnimationWithDuration(m_animator,
                                                     "GeodeShade_Anticipation",
                                                     TelegraphDuration);
            }
        }

        public override void OnRoamStart()
        {
            base.OnRoamStart();
            if (IsServer)
            {
                Utils.Anim.Play(m_animator, "GeodeShade_Roam");
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
                Utils.Anim.Play(m_animator, "GeodeShade_Roam");
            }
        }

        public override void OnAggroUpdate(float dt)
        {
            SimplePursueUpdate(dt);
        }

        public override void OnAttackStart()
        {
            SimpleAttackStart();
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, 0.1f);
            if (IsServer)
            {
                Utils.Anim.Play(m_animator, "GeodeShade_Attack");
            }
        }

        public override void OnCooldownStart()
        {
            if (IsServer)
            {
                Utils.Anim.Play(m_animator, "GeodeShade_Roam");
            }
        }

        public override void OnCooldownUpdate(float dt)
        {
            SimpleCooldownUpdate(dt);
        }
    }
}