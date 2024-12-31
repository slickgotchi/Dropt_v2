using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_LilEssence : EnemyAI
    {
        private Animator m_animator;
        public float EssenceReward = 10;
        [SerializeField] private List<SpriteRenderer> m_spritesToFlip;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public override void OnSpawnStart()
        {
            base.OnSpawnStart();
        }

        public override void OnTelegraphStart()
        {
        }

        public override void OnRoamUpdate(float dt)
        {
            SimpleRoamUpdate(dt);

            if (IsServer)
            {
                m_animator.Play("LilEssence_Idle");
            }
        }

        public override void OnFleeStart()
        {
            base.OnFleeStart();

            if (IsServer)
            {
                m_animator.Play("LilEssence_Alert");
            }
        }

        public override void OnFleeUpdate(float dt)
        {
            SimpleFleeUpdate(dt);
        }

        public override void OnAggroUpdate(float dt)
        {
            SimpleFleeUpdate(dt);
        }

        public override void OnAttackStart()
        {
        }

        public override void OnCooldownStart()
        {
        }

        public override void OnCooldownUpdate(float dt)
        {
        }

        public override void OnUpdate(float dt)
        {
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsServer) return;

            PlayerOffchainData playerData = collision.gameObject.GetComponent<PlayerOffchainData>();
            if (playerData != null)
            {
                playerData.AddEssence(EssenceReward);
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}
