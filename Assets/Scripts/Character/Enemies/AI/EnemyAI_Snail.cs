using UnityEngine;

namespace Dropt
{
    public class EnemyAI_Snail : EnemyAI
    {
        private Animator m_animator;
        private SoundFX_Snail m_soundFX_Snail;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            m_soundFX_Snail = GetComponent<SoundFX_Snail>();
        }

        public override void OnSpawnStart()
        {
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Spawn", SpawnDuration);
            }
            base.OnSpawnStart();
        }

        public override void OnTelegraphStart()
        {
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Telegraph", TelegraphDuration, interpolationDelay_s);
            }
        }

        public override void OnRoamStart()
        {
            base.OnRoamStart();
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Roam", TelegraphDuration, interpolationDelay_s);
            }
        }

        public override void OnRoamUpdate(float dt)
        {
            if (isStopped) return;

            SimpleRoamUpdate(dt);
        }

        public override void OnAggroStart()
        {
            base.OnAggroStart();
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Roam", TelegraphDuration, interpolationDelay_s);
            }
        }

        public override void OnAggroUpdate(float dt)
        {
            if (isStopped) return;

            SimplePursueUpdate(dt);
        }

        public override void OnAttackStart()
        {
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Attack", AttackDuration, interpolationDelay_s);
            }
            SimpleAttackStart();
            m_soundFX_Snail.PlayAttackSound();
            // set facing
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, AttackDuration);
        }

        public override void OnCooldownStart()
        {
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Roam", TelegraphDuration, interpolationDelay_s);
            }
        }

        public override void OnCooldownUpdate(float dt)
        {
            if (isStopped) return;

            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Roam", TelegraphDuration, interpolationDelay_s);
            }
            SimpleCooldownUpdate(dt);
        }
    }
}
