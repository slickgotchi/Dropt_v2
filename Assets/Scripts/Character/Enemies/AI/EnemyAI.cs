using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

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

        [Header("Durations")]
        public float SpawnDuration = 1f;
        public float TelegraphDuration = 1f;
        public float AttackDuration = 0.5f;
        public float CooldownDuration = 1f;

        [Header("Abilities")]
        public GameObject PrimaryAttack;
        public GameObject SecondaryAttack;

        [Header("NavMeshAgent Avoidance")]
        public float avoidanceRadius = 1f;

        [Header("Debug")]
        public EnemyAI_DebugCanvas debugCanvas;

        private float m_spawnTimer = 0f;
        private float m_telegraphTimer = 0f;
        private float m_attackTimer = 0f;
        private float m_cooldownTimer = 0f;
        private float m_stunTimer = 0f;

        // variables accessible to child classes
        protected Vector3 RoamAnchorPoint;
        protected NetworkCharacter networkCharacter;
        [HideInInspector] public NavMeshAgent navMeshAgent;


        [HideInInspector] public Vector3 AttackDirection;
        [HideInInspector] public Vector3 PositionToAttack;

        // variables set by the EnemyAIManager
        [HideInInspector] public GameObject NearestPlayer;
        [HideInInspector] public float NearestPlayerDistance;

        [HideInInspector] public float StunDuration;

        public enum State
        {
            Null,
            Spawn,
            Roam,
            Aggro,
            Telegraph,
            Attack,
            Cooldown,
            Stun,
            Flee,
        }

        [HideInInspector] public State state = State.Spawn;

        private void Awake()
        {
            Debug.Log(PrimaryAttack);
        }

        public override void OnNetworkSpawn()
        {
            networkCharacter = GetComponent<NetworkCharacter>();
            navMeshAgent = GetComponent<NavMeshAgent>();

            RoamAnchorPoint = transform.position;
            m_spawnTimer = SpawnDuration;
            state = State.Spawn;
            OnSpawnStart();
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            OnUpdate(dt);
            HandleDebugCanvas();

            if (!IsServer) return;
            if (NearestPlayer == null) return;


            switch (state)
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
                case State.Stun:
                    HandleStun(dt);
                    break;
                case State.Flee:
                    HandleFlee(dt);
                    break;
                default: break;
            }
        }

        public virtual void OnNullUpdate(float dt) { }

        public virtual void OnSpawnStart() { }
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

        public virtual void OnKnockback(Vector3 direction, float distance, float duration) { }

        public virtual void OnStunStart() { }
        public virtual void OnStunUpdate(float dt) { }
        public virtual void OnStunFinish() { }

        public virtual void OnFleeStart() { }
        public virtual void OnFleeUpdate(float dt) { }
        public virtual void OnFleeFinish() { }

        public virtual void OnUpdate(float dt) { }

        public virtual void OnDeath(Vector3 position) { }

        void HandleNull(float dt)
        {
            OnNullUpdate(dt);
        }

        void HandleSpawning(float dt)
        {
            m_spawnTimer -= dt;
            if (m_spawnTimer < 0)
            {
                OnSpawnFinish();
                state = State.Roam;
                OnRoamStart();
                return;
            }

            OnSpawnUpdate(dt);
        }

        void HandleRoam(float dt)
        {
            if (NearestPlayerDistance < FleeRange)
            {
                OnRoamFinish();
                state = State.Flee;
                OnFleeStart();
                return;
            }

            if (NearestPlayerDistance < AggroRange)
            {
                OnRoamFinish();
                state = State.Aggro;
                OnAggroStart();
                return;
            }

            OnRoamUpdate(dt);
        }

        void HandleAggro(float dt)
        {
            //if (NearestPlayerDistance < FleeRange)
            //{
            //    OnAggroFinish();
            //    state = State.Flee;
            //    OnFleeStart();
            //    return;
            //}

            if (NearestPlayerDistance > BreakAggroRange)
            {
                OnAggroFinish();
                state = State.Roam;
                OnRoamStart();
                return;
            }

            if (NearestPlayerDistance < AttackRange)
            {
                OnAggroFinish();
                state = State.Telegraph;
                OnTelegraphStart();
                m_telegraphTimer = TelegraphDuration;
                return;
            }

            OnAggroUpdate(dt);
        }

        void HandleTelegraph(float dt)
        {
            m_telegraphTimer -= dt;
            if (m_telegraphTimer < 0)
            {
                OnTelegraphFinish();
                state = State.Attack;
                OnAttackStart();
                m_attackTimer = AttackDuration;
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
                state = State.Cooldown;
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
                state = State.Aggro;
                OnAggroStart();
                return;
            }

            OnCooldownUpdate(dt);
        }

        void HandleStun(float dt)
        {
            m_stunTimer -= dt;
            if (m_stunTimer < 0)
            {
                OnStunFinish();
                state = m_postStunState;
                if (state == State.Aggro)
                {
                    OnAggroStart();
                }
                return;
            }

            OnStunUpdate(dt);
        }

        void HandleFlee(float dt)
        {
            if (NearestPlayerDistance > BreakFleeRange)
            {
                OnFleeFinish();
                state = State.Roam;
            }

            OnFleeUpdate(dt);
        }

        State m_postStunState = State.Aggro;

        public void Knockback(Vector3 direction, float distance, float stunTime)
        {
            // account for multipliers
            distance *= GetComponent<NetworkCharacter>().KnockbackMultiplier.Value;
            stunTime *= GetComponent<NetworkCharacter>().StunMultiplier.Value;
            
            // about stun timer parameters
            StunDuration = stunTime;
            m_stunTimer = stunTime;

            // handle things based on our state
            switch (state)
            {
                case State.Null:
                    OnKnockback(direction, distance, stunTime);
                    m_postStunState = State.Aggro;
                    break;
                case State.Spawn:
                    // do nothing, spawn is uninterruptible
                    break;
                case State.Roam:
                    OnKnockback(direction, distance, stunTime);
                    m_postStunState = State.Aggro;
                    break;
                case State.Aggro:
                    OnKnockback(direction, distance, stunTime);
                    m_postStunState = State.Aggro;
                    break;
                case State.Telegraph:
                    OnKnockback(direction, distance, stunTime);
                    m_postStunState = State.Aggro;
                    break;
                case State.Attack:
                    // do nothing, attacks are uninterruptible
                    break;
                case State.Cooldown:
                    OnKnockback(direction, distance, stunTime);
                    m_postStunState = State.Cooldown;
                    break;
                default: break;
            }

            // set our new state to stun
            state = State.Stun;
            OnStunStart();
        }


        void HandleDebugCanvas()
        {
            if (debugCanvas == null) return;

            debugCanvas.stateTMP.text = state.ToString();

            switch (state)
            {
                case State.Spawn:
                    debugCanvas.slider.value = m_spawnTimer / SpawnDuration;
                    break;
                case State.Telegraph:
                    debugCanvas.slider.value = m_telegraphTimer / TelegraphDuration;
                    break;
                case State.Attack:
                    debugCanvas.slider.value = m_attackTimer / AttackDuration;
                    break;
                case State.Cooldown:
                    debugCanvas.slider.value = m_cooldownTimer / CooldownDuration;
                    break;
                case State.Stun:
                    debugCanvas.slider.value = m_stunTimer / StunDuration;
                    break;
                default:
                    debugCanvas.slider.value = 0;
                    break;
            }
        }
    }
}
