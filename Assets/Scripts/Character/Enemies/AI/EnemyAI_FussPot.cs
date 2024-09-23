using Dropt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_FussPot : EnemyAI
    {
        [Header("FussPot Specific")]
        public float ProjectileSpreadInDegrees = 30;

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

        public override void OnTelegraphFinish()
        {
        }

        public override void OnRoamUpdate(float dt)
        {
        }

        public override void OnAggroUpdate(float dt)
        {
        }

        public override void OnAttackStart()
        {
            SimpleFussPotAttack();

            // set facing
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, AttackDuration);
        }

        // attack
        protected void SimpleFussPotAttack()
        {
            // check we have a primary attack.
            if (PrimaryAttack == null) return;

            // instantiate an attack
            var ability = Instantiate(PrimaryAttack);

            // get enemy ability of attack
            var enemyAbility = ability.GetComponent<EnemyAbility>();
            if (enemyAbility == null) return;

            // get fusspot erupt ability
            var fussPotErupt = ability.GetComponent<FussPot_Erupt>();
            fussPotErupt.ProjectileSpreadInDegrees = ProjectileSpreadInDegrees;

            // initialise the ability
            ability.GetComponent<NetworkObject>().Spawn();
            enemyAbility.Init(gameObject, NearestPlayer, AttackDirection, AttackDuration, PositionToAttack);
            enemyAbility.Activate();
        }
    }
}
