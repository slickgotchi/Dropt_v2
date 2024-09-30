using Dropt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_FudWisp : EnemyAI
    {
        [Header("FudWisp Specific")]
        public float ExplosionRadius = 2f;
        public float WaitForNonRootedPlayerRange = 4f;

        private Animator m_animator;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public override void OnSpawnStart()
        {
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
            //SimplePursueUpdate(dt);
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
            //SimplePursueUpdate(dt);
            FudWisp_PursueUpdate(dt);
        }

        //public override void OnKnockback(Vector3 direction, float distance, float duration)
        //{
        //    SimpleKnockback(direction, distance, duration);
        //}

        // attack
        protected void FudWisp_AttackStart()
        {
            // check we have a primary attack.
            if (PrimaryAttack == null) return;

            // instantiate an attack
            var ability = Instantiate(PrimaryAttack);
            
            // get enemy ability of attack
            var enemyAbility = ability.GetComponent<EnemyAbility>();
            if (enemyAbility == null) return;

            // set explosion radius
            var fudWisp_Explosion = ability.GetComponent<FudWisp_Explode>();
            fudWisp_Explosion.ExplosionRadius = ExplosionRadius;

            // initialise the ability
            ability.GetComponent<NetworkObject>().Spawn();
            enemyAbility.Init(gameObject, NearestPlayer, Vector3.zero, AttackDuration, PositionToAttack);
            enemyAbility.Activate();
        }

        protected void FudWisp_PursueUpdate(float dt)
        {
            if (networkCharacter == null) return;
            if (m_navMeshAgent == null) return;

            m_navMeshAgent.isStopped = false;

            // get direction from player to enemy and set a small offset
            var dir = (transform.position - NearestPlayer.transform.position).normalized;

            // check if player is rooted
            var characterStatus = NearestPlayer.GetComponent<CharacterStatus>();
            var offset = characterStatus.IsRooted() ? WaitForNonRootedPlayerRange : 0.9f;

            var offsetVector = dir * AttackRange * offset;

            m_navMeshAgent.SetDestination(NearestPlayer.transform.position + offsetVector);
            m_navMeshAgent.speed = networkCharacter.MoveSpeed.Value * PursueSpeedMultiplier;

            HandleAntiClumping();
            HandleAlertOthers();
        }
    }
}
