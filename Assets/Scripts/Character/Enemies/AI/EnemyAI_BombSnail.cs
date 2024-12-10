using UnityEngine;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_BombSnail : EnemyAI
    {
        private Animator m_animator;
        private NetworkVariable<bool> m_isTriggered = new NetworkVariable<bool>(false);
        private NetworkVariable<float> m_triggerTimer = new NetworkVariable<float>(3);
        private SoundFX_BombSnail m_soundFX_BombSnail;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            m_soundFX_BombSnail = GetComponent<SoundFX_BombSnail>();
        }

        public override void OnSpawnStart()
        {
            if (IsServer)
            {
                m_animator.Play("BombSnail_Spawn");
            }
            base.OnSpawnStart();
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
            PlayExplodeSoundClientRpc();
            SimpleAttackStart();
            // set facing
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, AttackDuration);

        }

        [ClientRpc]
        private void PlayExplodeSoundClientRpc()
        {
            m_soundFX_BombSnail.PlayExplodeSound();
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
