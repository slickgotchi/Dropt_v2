using UnityEngine;

namespace Dropt
{
    public class EnemyAI_Snail : EnemyAI
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
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Spawn", SpawnDuration);
            }
        }

        public override void OnTelegraphStart()
        {
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Telegraph", TelegraphDuration);
            }
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, TelegraphDuration);
        }

        public override void OnRoamStart()
        {
            base.OnRoamStart();
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Roam", TelegraphDuration);
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
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Roam", TelegraphDuration);
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
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Attack", AttackDuration);
            }
            SimpleAttackStart();
            // set facing
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, AttackDuration);
        }

        public override void OnCooldownStart()
        {
            // set facing
            //GetComponent<EnemyController>().SetFacingFromDirection(NearestPlayer.transform.position - transform.position, CooldownDuration);
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Roam", TelegraphDuration);
            }
        }

        public override void OnCooldownUpdate(float dt)
        {
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "Snail_Roam", TelegraphDuration);
            }
            SimplePursueUpdate(dt);
        }

        //public override void OnKnockback(Vector3 direction, float distance, float duration)
        //{
        //    SimpleKnockback(direction, distance, duration);

        //    // stop animator
        //    m_animator.Play("Snail_Idle");
        //}
    }
}
