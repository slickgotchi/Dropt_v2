using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

namespace Dropt
{
    public class EnemyAI_GasBag : EnemyAI
    {
        [Header("GasBag Specific")]
        public float OnDestroyPoisonCloudRadius = 2f;
        public float OnTouchPoisonInterval = 0.5f;
        public float OnTouchPoisonDamage = 1f;
        public float PoisonCloudDuration = 1f;
        private float m_onTouchPoisonDamageTimer = 0f;

        public Collider2D OnTouchPoisonCollider;

        private Animator m_animator;
        [SerializeField] private GameObject m_greenCloud;
        [SerializeField] private GameObject m_purpleCloud;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public override void OnSpawnStart()
        {
            base.OnSpawnStart();
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "GasBag_Spawn", SpawnDuration);
                AttackDuration += PoisonCloudDuration;
            }
        }

        // override OnDeath so we do not get the default DeSpawn
        protected override void OnDeath(Vector3 position)
        {
            // change to telegraph state
            ChangeState(State.Telegraph);

            // stop nav mesh
            GetComponent<NavMeshAgent>().isStopped = true;
        }

        public override void OnRoamStart()
        {
            base.OnRoamStart();
            if (IsServer)
            {
                Utils.Anim.Play(m_animator, "GasBag_Roam");
            }
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
            if (m_onTouchPoisonDamageTimer < 0 && (state.Value == State.Aggro || state.Value == State.Attack))
            {
                m_onTouchPoisonDamageTimer = OnTouchPoisonInterval;
                HandleOnTouchCollisions();
            }
        }

        public override void OnAttackStart()
        {
            base.OnAttackStart();
            Utils.Anim.Play(m_animator, "GasBag_Death");
            OnTouchPoisonCollider.enabled = false;
        }

        public void ShowPoisionCloud()
        {
            if (!IsServer)
            {
                return;
            }
            ShowPoisionCloudClientRpc();
            GenerateEnemyAbility();
        }

        [ClientRpc]
        private void ShowPoisionCloudClientRpc()
        {
            m_greenCloud.SetActive(true);
        }

        private void GenerateEnemyAbility()
        {
            if (PrimaryAttack == null) return;

            // instantiate an attack
            GameObject ability = Instantiate(PrimaryAttack);

            // get enemy ability of attack
            EnemyAbility enemyAbility = ability.GetComponent<EnemyAbility>();
            if (enemyAbility == null) return;

            // set explosion radius
            GasBag_Explode gasBagExplosion = ability.GetComponent<GasBag_Explode>();
            gasBagExplosion.ExplosionRadius = OnDestroyPoisonCloudRadius;
            gasBagExplosion.transform.position = transform.position;
            // initialise the ability
            ability.GetComponent<NetworkObject>().Spawn();
            enemyAbility.Init(gameObject,
                              NearestPlayer,
                              Vector3.zero,
                              PoisonCloudDuration,
                              PositionToAttack);
            enemyAbility.Activate();
        }

        // attack
        protected void GasBag_Attack(Vector3 position)
        {
            // switch to telegraph state
            ChangeState(State.Telegraph);
        }

        private void HandleOnTouchCollisions()
        {
            // sync colliders to current transform
            Physics2D.SyncTransforms();

            // do a collision check
            List<Collider2D> playerHitColliders = new List<Collider2D>();
            OnTouchPoisonCollider.OverlapCollider(PlayerAbility.GetContactFilter(new string[] { "PlayerHurt" }), playerHitColliders);
            foreach (Collider2D hit in playerHitColliders)
            {
                Transform player = hit.transform.parent;
                if (player.HasComponent<NetworkCharacter>())
                {
                    // apply damage
                    if (IsServer)
                    {
                        Utils.Anim.Play(m_animator, "GasBag_Attack");
                    }
                    player.GetComponent<NetworkCharacter>().TakeDamage(OnTouchPoisonDamage, false);
                }
            }

            // clear out colliders
            playerHitColliders.Clear();
        }
    }
}
