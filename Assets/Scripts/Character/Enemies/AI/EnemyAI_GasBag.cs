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
        public Collider2D EnemyHurtCollider;

        private Animator m_animator;
        [SerializeField] private GameObject m_greenCloud;
        [SerializeField] private GameObject m_purpleCloud;

        private SoundFX_Gasbag m_soundFX_Gasbag;

        private bool m_isExploded = false;
        private bool m_isAttackSoundPlaying = false;
        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            m_soundFX_Gasbag = GetComponent<SoundFX_Gasbag>();
        }

        public override void OnSpawnStart()
        {
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "GasBag_Spawn", SpawnDuration);
                AttackDuration += PoisonCloudDuration;
            }
            base.OnSpawnStart();
        }

        private bool m_isDeathTriggered = false;

        
        // override OnDeath so we do not get the default DeSpawn
        protected override void OnDeath(Vector3 position)
        {
            transform.position = position;
            //Debug.Log("OnDeath(): " + transform.position);

            // stop nav mesh
            if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
            {
                m_navMeshAgent.isStopped = true;
            }

            m_isDeathTriggered = true;
            SetDeathClientRpc();

            // change to telegraph state
            ChangeState(State.Telegraph);
        }

        [Rpc(SendTo.ClientsAndHost)]
        void SetDeathClientRpc()
        {
            m_isDeathTriggered = true;
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
            if (m_isExploded || m_isDeathTriggered) return;
            SimpleRoamUpdate(dt);
        }

        public override void OnAggroUpdate(float dt)
        {
            if (m_isExploded || m_isDeathTriggered) return;
            SimplePursueUpdate(dt);
        }

        public override void OnUpdate(float dt)
        {
            if (m_isDeathTriggered)
            {
                //Debug.Log(state.Value + " " + transform.position);
            }

            if (m_isExploded || m_isDeathTriggered) return;
            base.OnUpdate(dt);

            m_onTouchPoisonDamageTimer -= dt;
            if (m_onTouchPoisonDamageTimer < 0 && (state.Value == State.Aggro || state.Value == State.Attack))
            {
                m_onTouchPoisonDamageTimer = OnTouchPoisonInterval;
                HandleOnTouchCollisions();
            }
        }

        public override void OnTelegraphStart()
        {
            base.OnTelegraphStart();

            // stop nav mesh
            if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
            {
                m_navMeshAgent.isStopped = true;
            }

            //Debug.Log("OnTelegraphStart(): " + transform.position);
        }

        public override void OnTelegraphUpdate(float dt)
        {
            base.OnTelegraphUpdate(dt);


        }

        public override void OnAttackStart()
        {
            base.OnAttackStart();
            Utils.Anim.Play(m_animator, "GasBag_Death");
            OnTouchPoisonCollider.enabled = false;
            EnemyHurtCollider.enabled = false;
            m_isExploded = true;
            // stop nav mesh
            if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
            {
                m_navMeshAgent.isStopped = true;
            }

            DisableCollidersClientRpc();

            //Debug.Log("OnAttackStart(): " + transform.position);

            transform.position = GetKnockbackPosition();
        }

        public override void OnAttackUpdate(float dt)
        {
            base.OnAttackUpdate(dt);

            m_navMeshAgent.isStopped = true;

            transform.position = GetKnockbackPosition();
        }

        public override void OnLateUpdate(float dt)
        {
            base.OnLateUpdate(dt);

            // we do this to keep dead gasbag in same place after death (prevents slight listing of the character)
            if (IsClient && m_isDeathTriggered)
            {
                transform.position = GetKnockbackPosition();
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        void DisableCollidersClientRpc()
        {
            OnTouchPoisonCollider.enabled = false;
            EnemyHurtCollider.enabled = false;
        }

        // this function is called by the GasBag_Death animation
        public void ShowPoisionCloud()
        {
            if (!IsServer)
            {
                return;
            }

            ShowPoisionCloudClientRpc();
            //Debug.Log("GenerateEnemyAbility");
            GenerateEnemyAbility();
        }

        private void HideHpBar()
        {
            GameObject hpBar = transform.GetComponentInChildren<StatBarCanvas>().gameObject;
            hpBar.SetActive(false);
        }

        [ClientRpc]
        private void ShowPoisionCloudClientRpc()
        {
            HideHpBar();
            m_greenCloud.SetActive(true);
            m_soundFX_Gasbag.PlayPoisionCloudSound();
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
            //gasBagExplosion.ExplosionRadius = OnDestroyPoisonCloudRadius;
            //gasBagExplosion.transform.position = transform.position;
            gasBagExplosion.transform.position = GetKnockbackPosition() + new Vector3(0, 0.5f, 0);
            //Debug.Log("GenerateEnemyAbility() gasBagExplosion position: " + gasBagExplosion.transform.position);
            // initialise the ability
            ability.GetComponent<NetworkObject>().Spawn();
            enemyAbility.Activate(gameObject, NearestPlayer, Vector3.zero, PoisonCloudDuration, PositionToAttack);
        }

        private void HandleOnTouchCollisions()
        {
            // sync colliders to current transform
            Physics2D.SyncTransforms();

            // do a collision check
            List<Collider2D> playerHitColliders = new List<Collider2D>();
            OnTouchPoisonCollider.OverlapCollider(PlayerAbility.GetContactFilter(new string[] { "PlayerHurt" }), playerHitColliders);
            if (playerHitColliders.Count == 0)
            {
                m_isAttackSoundPlaying = false;
                if (IsServer)
                {
                    Utils.Anim.Play(m_animator, "GasBag_Roam");
                }
                return;
            }
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
                    if (!m_isAttackSoundPlaying)
                    {
                        m_isAttackSoundPlaying = true;
                        m_soundFX_Gasbag.PlayAttackSound();
                    }
                    player.GetComponent<NetworkCharacter>().TakeDamage(OnTouchPoisonDamage, false);
                }
            }

            // clear out colliders
            playerHitColliders.Clear();
        }
    }
}
