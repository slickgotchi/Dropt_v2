using Dropt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_LilEssence : EnemyAI
    {
        private Animator m_animator;
        public float EssenceReward = 10;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public override void OnSpawnStart()
        {
        }

        public override void OnTelegraphStart()
        {
        }
        
        public override void OnRoamUpdate(float dt)
        {
            SimpleRoamUpdate(dt);   
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
