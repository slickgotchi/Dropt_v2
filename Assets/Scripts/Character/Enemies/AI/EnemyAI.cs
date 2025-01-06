using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Netcode.Components;

namespace Dropt
{
    public partial class EnemyAI : NetworkBehaviour
    {
        [Header("Speed Multipliers")]
        public float PursueSpeedMultiplier = 2f;
        public float RoamSpeedMultiplier = 2f;
        public float FleeSpeedMultiplier = 5f;

        [Header("Ranges")]
        public float AggroRange = 6f;           // aggro enemy when player is within this range
        public float AlertRange = 5f;           // other enemies within this range when we are aggro'd will also aggro
        public float AttackRange = 1.5f;        // attack when player within this range
        public float BreakAggroRange = 10f;     // leave aggro mode when outside this range (make sure BreakAggro is calculated after Alerts)
        public float MaxRoamRange = 10f;        // the furthest enemy can roam from its anchor point
        public float FleeRange = 0f;
        public float BreakFleeRange = 12f;
        public float PursueStopShortRange = 0.5f;

        [Header("Durations")]
        public float SpawnDuration = 1f;
        public float TelegraphDuration = 1f;
        public float AttackDuration = 0.5f;
        public float CooldownDuration = 1f;
        //public float KnockbackDuration = 0.3f;

        [Header("Targeting")]
        public float TelegraphSelectTargetTimeRatio = 1f;

        [Header("Abilities")]
        public GameObject PrimaryAttack;
        public GameObject SecondaryAttack;

        [Header("NavMeshAgent Avoidance")]
        public float avoidanceRadius = 1f;

        [Header("Debug")]
        public EnemyAI_DebugCanvas debugCanvas;

        [HideInInspector] public float interpolationDelay_s = 0.1f;

        private float m_spawnTimer = 0f;
        private float m_telegraphTimer = 0f;
        private float m_attackTimer = 0f;
        private float m_cooldownTimer = 0f;
        private float m_knockbackTimer = 0f;
        private float m_stunTimer = 0f;

        private float k_knockbackDuration = 0.1f;
        private float m_knockbackDuration = 0.5f;
        private Vector3 m_knockbackStartPosition;
        private Vector3 m_knockbackFinishPosition;
        private State m_preKnockbackState = State.Aggro;

        // variables accessible to child classes
        protected Vector3 RoamAnchorPoint;
        protected NetworkCharacter networkCharacter;
        [HideInInspector] public NavMeshAgent m_navMeshAgent;
        protected NetworkTransform m_networkTransform;

        private SoundFX_Enemy m_soundFX_Enemy;

        [HideInInspector] public Vector3 AttackDirection;
        [HideInInspector] public Vector3 PositionToAttack;

        // variables set by the EnemyAIManager
        [HideInInspector] public GameObject NearestPlayer;
        [HideInInspector] public float NearestPlayerDistance;

        [HideInInspector] public float StunDuration;

        private bool m_isDead = false;

        private ProximityCulling m_proximityCulling;

        public enum State
        {
            Null,
            Spawn,
            Roam,
            Aggro,
            Telegraph,
            Attack,
            Cooldown,
            Knockback,
            Stun,
            Flee,
            PredictionToAuthorativeSmoothing,
            Dead
        }

        [HideInInspector] public NetworkVariable<State> state = new NetworkVariable<State>(State.Spawn);
        [HideInInspector] public NetworkVariable<float> debugSlider = new NetworkVariable<float>(0);

        [HideInInspector] public static List<EnemyAI> enemyAIs = new List<EnemyAI>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // WARNING: this interpolation delay is a guess, more monitoring required to see how well
            // it actually works in live games
            interpolationDelay_s = 0.3f;

            enemyAIs.Add(this);

            m_soundFX_Enemy = GetComponent<SoundFX_Enemy>();
            networkCharacter = GetComponent<NetworkCharacter>();
            m_navMeshAgent = GetComponent<NavMeshAgent>();
            m_networkTransform = GetComponent<NetworkTransform>();
            m_proximityCulling = GetComponent<ProximityCulling>();

            // Find the closest point on the NavMesh
            if (FindClosestNavMeshPosition(transform.position, out Vector3 navMeshPosition))
            {
                transform.position = navMeshPosition;
            }
            else
            {
                Debug.LogWarning("No valid NavMesh found near spawn position.");
            }

            RoamAnchorPoint = transform.position;
            m_spawnTimer = SpawnDuration;
            if (IsServer)
            {
                state.Value = State.Spawn;
            }

            OnSpawnStart();

            // set debug visibility
            var enemyAICanvas = GetComponentInChildren<EnemyAI_DebugCanvas>();
            enemyAICanvas.Container.SetActive(EnemyAIManager.Instance.IsDebugVisible);

            Init();
        }

        public override void OnNetworkDespawn()
        {
            enemyAIs.Remove(this);

            base.OnNetworkDespawn();
        }

        public void Init()
        {
            if (IsServer) state.Value = State.Spawn;
            m_isDead = false;
        }

        private void Update()
        {
            if (m_proximityCulling != null && m_proximityCulling.IsCulled) return;

            float dt = Time.deltaTime;

            OnUpdate(dt);
            HandleDebugCanvas();

            HandleClientPredictedKnockback(dt);
            HandleClientPredictedStun(dt);

            if (!IsServer) return;
            if (NearestPlayer == null) return;

            switch (state.Value)
            {
                case State.Null:
                    HandleNull(dt);
                    break;
                case State.Spawn:
                    HandleSpawning(dt);
                    break;
                case State.Roam:
                    HandleRoam(dt);
                    break;
                case State.Aggro:
                    HandleAggro(dt);
                    break;
                case State.Telegraph:
                    HandleTelegraph(dt);
                    break;
                case State.Attack:
                    HandleAttack(dt);
                    break;
                case State.Cooldown:
                    HandleCooldown(dt);
                    break;
                case State.Knockback:
                    HandleKnockback(dt);
                    break;
                case State.Stun:
                    HandleStun(dt);
                    break;
                case State.Flee:
                    HandleFlee(dt);
                    break;
                //case State.Dead:
                //    HandleDead(dt);
                //    break;
                default: break;
            }
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;

            OnLateUpdate(dt);


            HandlClientPredictionToAuthorativeSmoothing(dt);
        }

        public virtual void OnNullUpdate(float dt) { }

        public virtual void OnSpawnStart()
        {
            m_soundFX_Enemy?.PlaySpawnSound();
        }

        public virtual void OnSpawnUpdate(float dt) { }
        public virtual void OnSpawnFinish() { }

        public virtual void OnRoamStart() { }
        public virtual void OnRoamUpdate(float dt) { }
        public virtual void OnRoamFinish() { }

        public virtual void OnAggroStart() { }
        public virtual void OnAggroUpdate(float dt) { }
        public virtual void OnAggroFinish() { }

        public virtual void OnTelegraphStart() { }
        public virtual void OnTelegraphUpdate(float dt) { }
        public virtual void OnTelegraphFinish() { }

        public virtual void OnAttackStart() { }
        public virtual void OnAttackUpdate(float dt) { }
        public virtual void OnAttackFinish() { }

        public virtual void OnCooldownStart() { }
        public virtual void OnCooldownUpdate(float dt) { }
        public virtual void OnCooldownFinish() { }

        public virtual void OnKnockbackStart() { }
        public virtual void OnKnockbackUpdate(float dt) { }
        public virtual void OnKnockbackFinish() { }

        public virtual void OnStunStart() { }
        public virtual void OnStunUpdate(float dt) { }
        public virtual void OnStunFinish() { }

        public virtual void OnFleeStart() { }
        public virtual void OnFleeUpdate(float dt) { }
        public virtual void OnFleeFinish() { }

        public virtual void OnDeadStart() { }
        public virtual void OnDeadUpdate(float dt) { }
        public virtual void OnDeadFinish() { }

        public virtual void OnUpdate(float dt) { }
        public virtual void OnLateUpdate(float dt) { }

        // override this function in child class if you want to do something other than despawn
        protected virtual void OnDeath(Vector3 position)
        {
            var networkObject = GetComponent<NetworkObject>();
            if (networkObject == null) return;

            var levelSpawn = GetComponent<Level.LevelSpawn>();

            //Core.Pool.NetworkObjectPool.Instance.ReturnNetworkObject(
            //    networkObject, levelSpawn.prefab);
            //networkObject.Despawn(false);
            networkObject.Despawn();
        }

        public void Death(Vector3 position)
        {
            if (!m_isDead)
            {
                m_isDead = true;
                OnDeath(position);
                PlayDeathSoundClientRPC();
            }
        }

        [ClientRpc]
        private void PlayDeathSoundClientRPC()
        {
            m_soundFX_Enemy.PlayDieSound();
        }

        public void EnemyTakeDamage()
        {
            m_soundFX_Enemy.PlayTakeDamageSound();
        }

        private void HandleNull(float dt)
        {
            OnNullUpdate(dt);
        }

        private void HandleSpawning(float dt)
        {
            m_spawnTimer -= dt;
            if (m_spawnTimer < 0)
            {
                OnSpawnFinish();
                state.Value = State.Roam;
                OnRoamStart();
                return;
            }

            OnSpawnUpdate(dt);
        }

        private void HandleRoam(float dt)
        {
            if (NearestPlayerDistance < FleeRange)
            {
                OnRoamFinish();
                state.Value = State.Flee;
                OnFleeStart();
                return;
            }

            if (NearestPlayerDistance < AggroRange)
            {
                OnRoamFinish();
                state.Value = State.Aggro;
                OnAggroStart();
                return;
            }

            OnRoamUpdate(dt);
        }

        void HandleAggro(float dt)
        {
            if (NearestPlayerDistance > BreakAggroRange)
            {
                OnAggroFinish();
                state.Value = State.Roam;
                OnRoamStart();
                return;
            }

            if (NearestPlayerDistance < AttackRange)
            {
                OnAggroFinish();
                state.Value = State.Telegraph;
                OnTelegraphStart();
                m_telegraphTimer = TelegraphDuration;
                return;
            }

            OnAggroUpdate(dt);
        }

        private bool m_isTelegraphAttackTimeSelected = false;

        void HandleTelegraph(float dt)
        {
            m_telegraphTimer -= dt;

            // see if time to calculate attack direction and position
            var elapsedTime = TelegraphDuration - m_telegraphTimer;
            var alpha = elapsedTime / TelegraphDuration;
            if (alpha > TelegraphSelectTargetTimeRatio && !m_isTelegraphAttackTimeSelected)
            {
                m_isTelegraphAttackTimeSelected = true;
                CalculateAttackDirectionAndPosition();
            }

            if (m_telegraphTimer < 0)
            {
                // calc attack and direction if not yet done
                if (!m_isTelegraphAttackTimeSelected) CalculateAttackDirectionAndPosition();

                // finish telegraph
                OnTelegraphFinish();
                state.Value = State.Attack;
                OnAttackStart();
                m_attackTimer = AttackDuration;

                // reset telegraph attack time calc
                m_isTelegraphAttackTimeSelected = false;

                return;
            }

            OnTelegraphUpdate(dt);
        }

        void HandleAttack(float dt)
        {
            m_attackTimer -= dt;
            if (m_attackTimer < 0)
            {
                OnAttackFinish();
                state.Value = State.Cooldown;
                OnCooldownStart();
                m_cooldownTimer = CooldownDuration;
                return;
            }

            OnAttackUpdate(dt);
        }

        void HandleCooldown(float dt)
        {
            m_cooldownTimer -= dt;
            if (m_cooldownTimer < 0)
            {
                OnCooldownFinish();
                state.Value = State.Aggro;
                OnAggroStart();
                return;
            }

            OnCooldownUpdate(dt);
        }

        void HandleKnockback(float dt)
        {
            m_knockbackTimer += dt;
            var alpha = math.min(m_knockbackTimer / m_knockbackDuration, 1);
            transform.position = math.lerp(m_knockbackStartPosition, m_knockbackFinishPosition, alpha);

            if (m_knockbackTimer >= m_knockbackDuration)
            {
                OnKnockbackFinish();

                // set state based on if we're dead or not
                if (m_isDead)
                {
                    state.Value = State.Dead;
                    OnDeath(transform.position);
                    OnDeadStart();
                }
                else
                {
                    state.Value = State.Stun;
                    OnStunStart();
                    m_stunTimer = StunDuration;
                }

                return;
            }
        }

        void HandleStun(float dt)
        {
            m_stunTimer -= dt;
            if (m_stunTimer < 0)
            {
                OnStunFinish();
                state.Value = State.Aggro;
                OnAggroStart();
                return;
            }

            OnStunUpdate(dt);
        }

        void HandleFlee(float dt)
        {
            if (NearestPlayerDistance > BreakFleeRange)
            {
                OnFleeFinish();
                state.Value = State.Roam;
            }

            OnFleeUpdate(dt);
        }

        void HandleDead(float dt)
        {
            // do nothing
            OnDeadUpdate(dt);
        }

        public void Knockback(Vector3 direction, float distance, float stunTime)
        {
            if (state.Value == State.Attack) return;
            if (state.Value == State.Spawn) return;

            // get network character
            var networkCharacter = GetComponent<NetworkCharacter>();
            if (networkCharacter == null) return;

            // if we're dead, don't do anymore knockback
            if (networkCharacter.currentDynamicStats.IsDead) return;

            // account for multipliers
            distance *= networkCharacter.currentStaticStats.KnockbackMultiplier;
            stunTime *= networkCharacter.currentStaticStats.StunMultiplier;

            // recalc distance allowing for collisions
            distance = CalculateKnockbackDistance(direction, distance);

            // set stun timer parameters
            StunDuration = stunTime;
            m_stunTimer = stunTime;

            // reset knockback timer and set start and finish positions
            m_knockbackTimer = 0f;
            m_knockbackStartPosition = transform.position;
            m_knockbackFinishPosition = transform.position + direction.normalized * distance;

            // stop navmesh agent
            if (m_navMeshAgent != null && m_navMeshAgent.isOnNavMesh)
            {
                m_navMeshAgent.isStopped = true;
            }

            // save pre knockback state
            m_preKnockbackState = state.Value;

            // start knockback state
            if (IsServer)
            {
                StartState(State.Knockback, k_knockbackDuration);
            }
            else if (IsClient)
            {
                m_clientPredictedState = State.Knockback;
                if (m_networkTransform != null) m_networkTransform.enabled = false;
                m_knockbackDuration = k_knockbackDuration;
            }
        }

        public Vector3 GetKnockbackPosition()
        {
            return m_knockbackFinishPosition;
        }

        State m_clientPredictedState = State.Null;

        public State GetClientPredictedState() { return m_clientPredictedState; }

        void HandleClientPredictedKnockback(float dt)
        {
            if (!IsClient) return;
            if (m_clientPredictedState != State.Knockback) return;

            // check if player is dead
            var networkCharacter = GetComponent<NetworkCharacter>();
            if (networkCharacter == null) return;
            if (networkCharacter.currentDynamicStats.IsDead) return;

            m_knockbackTimer += dt;
            var alpha = math.min(m_knockbackTimer / m_knockbackDuration, 1);
            transform.position = math.lerp(m_knockbackStartPosition, m_knockbackFinishPosition, alpha);

            if (m_knockbackTimer >= m_knockbackDuration)
            {
                m_clientPredictedState = State.Stun;
                m_predictedStunFinishPosition = transform.position;
                if (m_networkTransform != null) m_stunTimer = StunDuration;
                return;
            }
        }

        void HandleClientPredictedStun(float dt)
        {
            if (!IsClient) return;
            if (m_clientPredictedState != State.Stun) return;

            // check if player is dead
            var networkCharacter = GetComponent<NetworkCharacter>();
            if (networkCharacter == null) return;
            if (networkCharacter.currentDynamicStats.IsDead) return;

            m_stunTimer -= dt;
            if (m_stunTimer < 0)
            {
                m_clientPredictedState = State.PredictionToAuthorativeSmoothing;
                m_smoothingTimer = 0f;
                transform.position = m_predictedStunFinishPosition;
                if (m_networkTransform != null) m_networkTransform.enabled = true;
                return;
            }
        }

        float m_smoothingTimer = 0f;
        float k_smoothingDuration = 1f;
        Vector3 m_predictedStunFinishPosition;

        void HandlClientPredictionToAuthorativeSmoothing(float dt)
        {
            if (!IsClient) return;
            if (m_clientPredictedState != State.PredictionToAuthorativeSmoothing) return;

            var networkTransformPosition = transform.position;

            m_smoothingTimer += dt;
            var alpha = math.min(m_smoothingTimer / k_smoothingDuration, 1);

            transform.position = math.lerp(m_predictedStunFinishPosition, networkTransformPosition, alpha);

            if (m_smoothingTimer >= k_smoothingDuration)
            {
                m_clientPredictedState = State.Null;
            }
        }

        private float CalculateKnockbackDistance(Vector3 direction, float distance)
        {
            // Create a temporary circle collider
            CircleCollider2D tempCircle = gameObject.AddComponent<CircleCollider2D>();
            tempCircle.isTrigger = true;  // Temporary collider is just for detection
            tempCircle.radius = 0.5f;

            // Cast the circle along the direction vector
            RaycastHit2D[] hits = new RaycastHit2D[10];  // Array to store results
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("EnvironmentWall", "EnvironmentWater"));
            filter.useTriggers = false;

            // Cast the collider
            int hitCount = tempCircle.Cast(direction, filter, hits, distance);

            if (hitCount > 0)
            {
                // Adjust the distance to the first hit point
                distance = hits[0].distance;
            }

            // Clean up by removing the temporary collider
            Destroy(tempCircle);

            // Return the adjusted distance
            return distance;
        }

        void HandleDebugCanvas()
        {
            if (debugCanvas == null || !debugCanvas.isVisible) return;

            if (IsServer)
            {
                switch (state.Value)
                {
                    case State.Spawn:
                        debugSlider.Value = m_spawnTimer / SpawnDuration;
                        break;
                    case State.Telegraph:
                        debugSlider.Value = m_telegraphTimer / TelegraphDuration;
                        break;
                    case State.Attack:
                        debugSlider.Value = m_attackTimer / AttackDuration;
                        break;
                    case State.Cooldown:
                        debugSlider.Value = m_cooldownTimer / CooldownDuration;
                        break;
                    case State.Stun:
                        debugSlider.Value = m_stunTimer / StunDuration;
                        break;
                    default:
                        debugSlider.Value = 0;
                        break;
                }
            }

            if (IsClient && debugCanvas != null)
            {
                debugCanvas.stateTMP.text = state.Value.ToString();
                debugCanvas.slider.value = debugSlider.Value;
            }
        }

        protected void FinishState(State state)
        {
            if (!IsServer) return;

            switch (state)
            {
                case State.Spawn: OnSpawnFinish(); break;
                case State.Roam: OnRoamFinish(); break;
                case State.Aggro: OnAggroFinish(); break;
                case State.Telegraph: OnTelegraphFinish(); break;
                case State.Attack: OnAttackFinish(); break;
                case State.Cooldown: OnCooldownFinish(); break;
                case State.Knockback: OnKnockbackFinish(); break;
                case State.Stun: OnStunFinish(); break;
                case State.Flee: OnFleeFinish(); break;
                default: break;
            }
        }

        protected void StartState(State newState, float duration = -1f)
        {
            if (!IsServer) return;

            // set the new state
            state.Value = newState;

            // perform the new state On function
            switch (newState)
            {
                case State.Spawn:
                    OnSpawnStart();
                    m_spawnTimer = duration < 0 ? SpawnDuration : duration;
                    break;
                case State.Roam: OnRoamStart(); break;
                case State.Aggro: OnAggroStart(); break;
                case State.Telegraph:
                    OnTelegraphStart();
                    m_telegraphTimer = duration < 0 ? TelegraphDuration : duration;
                    break;
                case State.Attack:
                    OnAttackStart();
                    m_attackTimer = duration < 0 ? AttackDuration : duration;
                    break;
                case State.Cooldown:
                    OnCooldownStart();
                    m_cooldownTimer = duration < 0 ? CooldownDuration : duration;
                    break;
                case State.Knockback:
                    OnKnockbackStart();
                    m_knockbackDuration = duration < 0 ? k_knockbackDuration : duration;
                    m_knockbackTimer = 0;
                    break;
                case State.Stun:
                    OnStunStart();
                    m_stunTimer = duration < 0 ? StunDuration : duration;
                    break;
                case State.Flee: OnFleeStart(); break;
                default: break;
            }
        }

        public void ChangeState(State newState, float newDuration = -1f)
        {
            if (!IsServer) return;

            FinishState(state.Value);
            StartState(newState, newDuration);
        }

        private bool FindClosestNavMeshPosition(Vector3 origin, out Vector3 navMeshPosition, float maxNavMeshDistance = 5f)
        {
            // Sample the NavMesh to find the closest point within the specified range
            NavMeshHit hit;
            if (NavMesh.SamplePosition(origin, out hit, maxNavMeshDistance, NavMesh.AllAreas))
            {
                navMeshPosition = hit.position;
                return true; // Found a valid NavMesh surface
            }

            navMeshPosition = Vector3.zero;
            return false; // No valid NavMesh surface found
        }

    }
}
