using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace Level.Traps
{
    public abstract class Trap : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<int> Group;
        [HideInInspector]
        public NetworkVariable<int> MaxGroup;

        [SerializeField] protected BoxCollider2D m_collider;
        [SerializeField] protected float m_cooldownDuration;
        [SerializeField] protected float m_damage;
        [SerializeField] protected BuffDamageAbility m_buffDamageAbility;

        private float m_cooldownTimer;

        //is available trap for attack
        protected abstract bool IsAvailableForAttack
        {
            get;
        }

        private void Awake()
        {
            m_cooldownTimer = 0;
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

                //cause damage
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

        //setup group order
        public void SetupGroup(int group, int maxGroup)
        {
            Group.Value = group;
            MaxGroup.Value = maxGroup + 1;
        }
    }
}