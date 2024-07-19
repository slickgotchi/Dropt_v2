using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace Level.Traps
{
    public abstract class Trap : NetworkBehaviour
    {
        [SerializeField] protected BoxCollider2D m_collider;
        [SerializeField] protected float m_cooldownDuration;
        [SerializeField] protected float m_damage;
        [SerializeField] protected BuffDamageAbility m_buffDamageAbility;

        protected TrapsGroupSpawner m_group;
        protected int m_currentGroup;
        private float m_cooldownTimer;

        protected abstract bool IsAvailableForAttack
        {
            get;
        }

        private void Awake()
        {
            m_cooldownTimer = 0;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
        }

        protected virtual void Update()
        {
            if (IsServer)
            {
                if (!IsAvailableForAttack)
                {
                    m_cooldownTimer = 0;
                    return;
                }

                m_cooldownTimer -= Time.deltaTime;

                if (m_cooldownTimer > 0)
                    return;

                List<NetworkCharacter> result = ListPool<NetworkCharacter>.Get();
                result.Clear();
                EnemyAbility.FillPlayerCollisionCheckAndDamage(result, m_collider, m_damage, false, gameObject);

                foreach (var character in result)
                {
                    m_cooldownTimer = m_cooldownDuration;
                    m_buffDamageAbility?.Damage(character);
                }

                ListPool<NetworkCharacter>.Release(result);
            }
        }

        public virtual void SetupGroup(TrapsGroupSpawner spawner, int group)
        {
            m_currentGroup = group;
            m_group = spawner;
            spawner.AddChild(group);
        }
    }
}