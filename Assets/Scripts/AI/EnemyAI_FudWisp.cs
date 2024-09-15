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
            // calc our attack direction
            CalculateAttackDirection();

            // set our facing direction
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, TelegraphDuration);
        }
        
        public override void OnRoamUpdate(float dt)
        {
            SimpleRoamUpdate(dt);   
        }

        public override void OnAggroUpdate(float dt)
        {
            SimplePursueUpdate(dt);
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
            SimplePursueUpdate(dt);
        }

        public override void OnKnockback(Vector3 direction, float distance, float duration)
        {
            SimpleKnockback(direction, distance, duration);
        }

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
            enemyAbility.Init(gameObject, NearestPlayer, AttackDuration);
            enemyAbility.Activate();
        }
    }
}
