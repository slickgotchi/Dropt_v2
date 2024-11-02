using UnityEngine;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_BombSnail : EnemyAI
    {
        private Animator m_animator;

        private NetworkVariable<bool> m_isTriggered = new NetworkVariable<bool>(false);
        private NetworkVariable<float> m_triggerTimer = new NetworkVariable<float>(3);

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public override void OnSpawnStart()
        {
            if (IsServer)
            {
                m_animator.Play("BombSnail_Spawn");
            }
        }

        public override void OnTelegraphStart()
        {
            //if (IsServer)
            //{
            //    m_animator.Play("BombSnail_Explosion");
            //}
        }

        public override void OnRoamUpdate(float dt)
        {
            if (IsServer)
            {
                m_animator.Play("BombSnail_Roam");
            }
            SimpleRoamUpdate(dt);
        }

        public override void OnAggroUpdate(float dt)
        {
            SimplePursueUpdate(dt);
        }

        public override void OnAttackStart()
        {
            //Debug.Log("ATTACK START");
            SimpleAttackStart();
            // set facing
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, AttackDuration);
        }

        public override void OnCooldownStart()
        {
        }

        public override void OnCooldownUpdate(float dt)
        {
            SimpleCooldownUpdate(dt);
        }
    }
}
