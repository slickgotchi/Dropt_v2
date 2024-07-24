using System.Collections.Generic;
using Character.Player;
using Unity.Netcode;
using UnityEngine;

namespace Level.Traps
{
    public abstract class Trap : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<int> Group;
        [HideInInspector]
        public NetworkVariable<int> MaxGroup;

        [SerializeField] protected float m_cooldownDuration;

        protected TrapsGroup m_group;

        //setup group order
        public void SetupGroup(TrapsGroup spawner, int group, int maxGroup)
        {
            m_group = spawner;
            Group.Value = group;
            MaxGroup.Value = maxGroup + 1;
        }

        protected virtual void Update()
        {
        }
    }

    public abstract class DamagedTrap : Trap
    {
        [SerializeField] protected float m_damage;
        [SerializeField] protected BuffDamageAbility m_buffDamageAbility;

        private readonly HashSet<NetworkCharacter> m_players;

        //is available trap for attack
        protected abstract bool IsAvailableForAttack
        {
            get;
        }

        protected DamagedTrap()
        {
            m_players = new HashSet<NetworkCharacter>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var player = collision.gameObject.GetComponent<NetworkCharacter>();

            if (player == null)
                return;

            m_players.Add(player);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            var player = collision.gameObject.GetComponent<NetworkCharacter>();

            if (player == null)
                return;

            m_players.Remove(player);
        }

        protected override void Update()
        {
            if (null == m_group)
            {
                m_group = new TrapsGroup();
                m_group.Traps.Add(this);
            }

            m_group.TryToUpdateCooldown(this);

            if (!IsAvailableForAttack)
            {
                return;
            }
            
            if (m_group.CooldownTimer > 0 || m_players.Count == 0)
                return;

            m_group.ResetCooldown(m_cooldownDuration);

            //cause damage

            foreach (var player in m_players)
            {
                if (IsServer)
                {
                    player.TakeDamage(m_damage, false, gameObject);
                    m_buffDamageAbility?.Damage(player);
                }
                else
                {
                    player.GetComponent<PlayerStepSynchronization>().WaitUntilReceiveServerData();
                }
            }
        }
    }
}