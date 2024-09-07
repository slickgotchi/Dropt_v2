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

        [Header("Ranges")]
        public float AggroRange = 6f;           // aggro enemy when player is within this range
        public float AlertRange = 5f;           // other enemies within this range when we are aggro'd will also aggro
        public float AttackRange = 1.5f;        // attack when player within this range
        public float BreakAggroRange = 10f;     // leave aggro mode when outside this range (make sure BreakAggro is calculated after Alerts)
        public float MaxRoamRange = 10f;        // the furthest enemy can roam from its anchor point

        [Header("Durations")]
        public float SpawnDuration = 1f;
        public float TelegraphDuration = 1f;
        public float AttackDuration = 0.5f;
        public float CooldownDuration = 1f;

        [Header("Abilities")]
        public GameObject PrimaryAttack;
        public GameObject SecondaryAttack;

        [Header("Debug")]
        public EnemyAI_DebugCanvas debugCanvas;

        private float m_spawnTimer = 0f;
        private float m_telegraphTimer = 0f;
        private float m_attackTimer = 0f;
        private float m_cooldownTimer = 0f;

        // variables accessible to child classes
        protected Vector3 RoamAnchorPoint;
        protected NetworkCharacter NetworkCharacter;
        protected NavMeshAgent NavMeshAgent;

        [HideInInspector] public Vector3 AttackDirection;

        // variables set by the EnemyAIManager
        [HideInInspector] public GameObject NearestPlayer;
        [HideInInspector] public float NearestPlayerDistance;

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
        }

        public State state = State.Spawn;

        private void Awake()
        {
            Debug.Log(PrimaryAttack);
        }

        public override void OnNetworkSpawn()
        {
            NetworkCharacter = GetComponent<NetworkCharacter>();
            NavMeshAgent = GetComponent<NavMeshAgent>();

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
                case State.Knockback:
                    HandleKnockback(dt);
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

        public virtual void OnKnockbackStart() { }
        public virtual void OnKnockbackUpdate(float dt) { }
        public virtual void OnKnockbackFinish() { }

        public virtual void OnUpdate(float dt) { }


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
            }

            OnSpawnUpdate(dt);
        }

        void HandleRoam(float dt)
        {
            if (NearestPlayerDistance < AggroRange)
            {
                OnRoamFinish();
                state = State.Aggro;
                OnAggroStart();
            }

            OnRoamUpdate(dt);
        }

        void HandleAggro(float dt)
        {
            if (NearestPlayerDistance > BreakAggroRange)
            {
                OnAggroFinish();
                state = State.Roam;
                OnRoamStart();
            }

            if (NearestPlayerDistance < AttackRange)
            {
                OnAggroFinish();
                state = State.Telegraph;
                OnTelegraphStart();
                m_telegraphTimer = TelegraphDuration;
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
            }

            OnAttackUpdate(dt);
        }

        void HandleCooldown(float dt)
        {
            m_cooldownTimer -= dt;
            if (m_cooldownTimer < 0)
            {
                OnCooldownFinish();
                state = State.Roam;
                OnRoamStart();
            }

            OnCooldownUpdate(dt);
        }

        void HandleKnockback(float dt)
        {

        }


        void HandleDebugCanvas()
        {
            if (debugCanvas == null) return;

            debugCanvas.stateTMP.text = state.ToString();

            if (state == State.Spawn)
            {
                debugCanvas.slider.value = m_spawnTimer / SpawnDuration;
            }
            else if (state == State.Telegraph)
            {
                debugCanvas.slider.value = m_telegraphTimer / TelegraphDuration;
            }
            else if (state == State.Attack)
            {
                debugCanvas.slider.value = m_attackTimer / AttackDuration;
            }
            else if (state == State.Cooldown)
            {
                debugCanvas.slider.value = m_cooldownTimer / CooldownDuration;
            }
            else
            {
                debugCanvas.slider.value = 0;
            }
        }
    }
}
