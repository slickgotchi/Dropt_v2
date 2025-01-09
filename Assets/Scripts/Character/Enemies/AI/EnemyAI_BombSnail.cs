using UnityEngine;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_BombSnail : EnemyAI
    {
        private Animator m_animator;
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
                //m_animator.Play("BombSnail_Spawn");
                Utils.Anim.Play(m_animator, "BombSnail_Spawn");
            }
            base.OnSpawnStart();
        }

        public override void OnTelegraphStart()
        {

        }

        public override void OnRoamUpdate(float dt)
        {
            if (IsServer)
            {
                //m_animator.Play("BombSnail_Roam");
                Utils.Anim.Play(m_animator, "BombSnail_Spawn", interpolationDelay_s);
            }
            SimpleRoamUpdate(dt);
        }

        public override void OnAggroStart()
        {
            base.OnAggroStart();

            if (IsServer)
            {
                Utils.Anim.Play(m_animator, "BombSnail_ShortFuse");
                PlayIgniteSoundClientRpc();
            }
        }

        [ClientRpc]
        private void PlayIgniteSoundClientRpc()
        {
            m_soundFX_BombSnail.PlayIgniteSound();
        }

        public override void OnAggroUpdate(float dt)
        {
            SimplePursueUpdate(dt);
        }

        public override void OnAttackStart()
        {
            PlayExplodeSoundClientRpc();
            SimpleAttackStart();
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
