using Dropt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_GasBag : EnemyAI
    {
        [Header("GasBag Specific")]
        public float OnDestroyPoisonCloudRadius = 2f;
        public float OnTouchPoisonInterval = 0.5f;
        public float OnTouchPoisonDamage = 1f;

        private float m_onTouchPoisonDamageTimer = 0f;

        public Collider2D OnTouchPoisonCollider;

        private Animator m_animator;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public override void OnDeath(Vector3 position)
        {
            // do gas bag explode attack when despawning
            GasBagExplodeAttack(position);
        }

        public override void OnRoamUpdate(float dt)
        {
            SimpleRoamUpdate(dt);   
        }

        public override void OnAggroUpdate(float dt)
        {
            SimplePursueUpdate(dt);
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            m_onTouchPoisonDamageTimer -= dt;
            if (m_onTouchPoisonDamageTimer < 0)
            {
                m_onTouchPoisonDamageTimer = OnTouchPoisonInterval;

                HandleOnTouchCollisions();
            }
        }

        public override void OnKnockback(Vector3 direction, float distance, float duration)
        {
            SimpleKnockback(direction, distance, duration);
        }

        // attack
        protected void GasBagExplodeAttack(Vector3 position)
        {
            // check we have a primary attack.
            if (PrimaryAttack == null) return;

            // instantiate an attack
            var ability = Instantiate(PrimaryAttack);
            
            // get enemy ability of attack
            var enemyAbility = ability.GetComponent<EnemyAbility>();
            if (enemyAbility == null) return;

            // set explosion radius
            var gasBagExplosion = ability.GetComponent<GasBag_Explode>();
            gasBagExplosion.ExplosionRadius = OnDestroyPoisonCloudRadius;
            gasBagExplosion.transform.position = transform.position;

            // initialise the ability
            ability.GetComponent<NetworkObject>().Spawn();
            enemyAbility.Init(gameObject, NearestPlayer, Vector3.zero, AttackDuration);
            enemyAbility.Activate();
        }

        private void HandleOnTouchCollisions()
        {
            // sync colliders to current transform
            Physics2D.SyncTransforms();

            // do a collision check
            List<Collider2D> playerHitColliders = new List<Collider2D>();
            OnTouchPoisonCollider.OverlapCollider(PlayerAbility.GetContactFilter(new string[] { "PlayerHurt" }), playerHitColliders);
            foreach (var hit in playerHitColliders)
            {
                var player = hit.transform.parent;
                if (player.HasComponent<NetworkCharacter>())
                {
                    // apply damage
                    player.GetComponent<NetworkCharacter>().TakeDamage(OnTouchPoisonDamage, false);
                }
            }

            // clear out colliders
            playerHitColliders.Clear();
        }
    }
}
