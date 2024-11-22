using UnityEngine;
using Unity.Netcode;
using System;

namespace Dropt
{
    public class EnemyAI_FudWisp : EnemyAI
    {
        [Header("FudWisp Specific")]
        public float WaitForNonRootedPlayerRange = 4f;
        private Action<GameObject> m_onFudWispDespawn;

        private SoundFX_FudWisp m_soundFX_FudWisp;

        private void Awake()
        {
            m_soundFX_FudWisp = GetComponent<SoundFX_FudWisp>();
        }

        public override void OnSpawnStart()
        {
            base.OnSpawnStart();
        }

        public void AssignDespawnAction(Action<GameObject> onFudWispDespawn)
        {
            m_onFudWispDespawn = onFudWispDespawn;
        }

        public override void OnTelegraphStart()
        {
            // set our facing direction
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, TelegraphDuration);
        }

        public override void OnRoamUpdate(float dt)
        {
            SimpleRoamUpdate(dt);
        }

        public override void OnAggroUpdate(float dt)
        {
            FudWisp_PursueUpdate(dt);
        }

        public override void OnAttackStart()
        {
            FudWisp_AttackStart();
            // set facing
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, AttackDuration);
        }

        public override void OnCooldownStart()
        {
        }

        public override void OnCooldownUpdate(float dt)
        {
            FudWisp_PursueUpdate(dt);
        }

        protected override void OnDeath(Vector3 position)
        {
            FudWisp_AttackStart();
            //base.OnDeath(position);
        }

        // attack
        protected void FudWisp_AttackStart()
        {
            // check we have a primary attack.
            if (PrimaryAttack == null) return;

            // instantiate an attack
            GameObject ability = Instantiate(PrimaryAttack, transform.position, Quaternion.identity);

            // get enemy ability of attack
            EnemyAbility enemyAbility = ability.GetComponent<EnemyAbility>();
            if (enemyAbility == null) return;

            // initialise the ability           
            ability.GetComponent<NetworkObject>().Spawn();
            enemyAbility.Activate(gameObject, NearestPlayer, Vector3.zero, AttackDuration, PositionToAttack);
            m_soundFX_FudWisp.PlayExplodeSound();
        }

        protected void FudWisp_PursueUpdate(float dt)
        {
            if (networkCharacter == null || m_navMeshAgent == null)
            {
                return;
            }

            m_navMeshAgent.isStopped = false;

            // get direction from player to enemy and set a small offset
            Vector3 dir = (transform.position - NearestPlayer.transform.position).normalized;

            // check if player is rooted
            CharacterStatus characterStatus = NearestPlayer.GetComponent<CharacterStatus>();
            float offset = characterStatus.IsRooted() ? WaitForNonRootedPlayerRange : 0.9f;

            Vector3 offsetVector = AttackRange * offset * dir;
            m_navMeshAgent.SetDestination(NearestPlayer.transform.position + offsetVector);
            m_navMeshAgent.speed = networkCharacter.MoveSpeed.Value * PursueSpeedMultiplier;

            HandleAntiClumping();
            HandleAlertOthers();
        }

        public override void OnNetworkDespawn()
        {
            m_onFudWispDespawn?.Invoke(gameObject);
            base.OnNetworkDespawn();
        }
    }
}
