using UnityEngine;

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
            Utils.Anim.PlayAnimationWithDuration(m_animator, "LeafShade_Spawn", SpawnDuration);
        }

        public override void OnTelegraphStart()
        {
            if (IsServer)
            {
                m_navMeshAgent.isStopped = true;
                Utils.Anim.PlayAnimationWithDuration(m_animator, "LeafShade_Anticipation", TelegraphDuration);
            }
        }

        public override void OnRoamStart()
        {
            base.OnRoamStart();
            if (IsServer)
            {
                Utils.Anim.Play(m_animator, "LeafShade_Roam");
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
                Utils.Anim.Play(m_animator, "LeafShade_Roam");
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
                Utils.Anim.Play(m_animator, "LeafShade_Attack");
            }
            SimpleAttackStart();
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, AttackDuration);
        }

        public override void OnCooldownStart()
        {
            if (IsServer)
            {
                Utils.Anim.Play(m_animator, "LeafShade_Roam");
            }
        }

        public override void OnCooldownUpdate(float dt)
        {
            SimplePursueUpdate(dt);
        }
    }
}