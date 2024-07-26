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

        protected TrapsGroup m_group;
        protected float m_cooldownTimer;

        protected virtual void Update()
        {
        }

        //setup group order
        public void SetupGroup(TrapsGroup spawner, int group, int maxGroup)
        {
            m_group = spawner;
            Group.Value = group;
            MaxGroup.Value = maxGroup + 1;
        }
    }

    public abstract class DamagedTrap : Trap
    {
        [SerializeField] protected float m_damage;
        [SerializeField] protected BuffDamageAbility m_buffDamageAbility;

        //is available trap for attack
        protected abstract bool IsAvailableForAttack
        {
            get;
        }

        private void Awake()
        {
            m_cooldownTimer = 0;
        }

        protected override void Update()
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
                EnemyAbility.FillPlayerCollisionWithBottomCheckAndDamage(result, m_collider, m_damage, false, gameObject);

                foreach (var character in result)
                {
                    m_cooldownTimer = m_cooldownDuration;
                    m_buffDamageAbility?.Damage(character);
                    ActivateDamage();
                }

                ListPool<NetworkCharacter>.Release(result);
            }
        }

        protected virtual void ActivateDamage()
        {
        }
    }
}