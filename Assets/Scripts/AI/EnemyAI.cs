using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Dropt
{
    public class EnemyAI : NetworkBehaviour
    {
        [Header("Speed Multipliers")]
        public float PursueSpeedMultiplier = 2f;
        public float RoamSpeedMultiplier = 2f;

        [Header("Ranges")]
        public float AggroRange = 6f;
        public float AttackRange = 1.5f;
        public float BreakAggroRange = 10f;

        [Header("Durations")]
        public float SpawnDuration = 1f;
        public float TelegraphDuration = 1f;
        public float AttackDuration = 0.5f;
        public float CooldownDuration = 1f;

        [Header("Abilities")]
        public GameObject Attack;

        private float m_spawnTimer = 0f;
        private float m_telegraphTimer = 0f;
        private float m_attackTimer = 0f;
        private float m_cooldownTimer = 0f;

        protected Vector3 RoamAnchorPoint;

        [HideInInspector] public GameObject NearestPlayer;
        [HideInInspector] public float NearestPlayerDistance;

        public enum State
        {
            Null,
            Spawning,
            Roam,
            Aggro,
            Telegraph,
            Attack,
            Cooldown,
            Knockback,
        }

        public State state = State.Spawning;

        public override void OnNetworkSpawn()
        {
            RoamAnchorPoint = transform.position;
            m_spawnTimer = SpawnDuration;
            state = State.Spawning;
        }

        private void Update()
        {
            if (!IsServer) return;
            if (NearestPlayer == null) return;

            float dt = Time.deltaTime;

            switch (state)
            {
                case State.Null:
                    HandleNull(dt);
                    break;                
                case State.Spawning:
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

        public virtual void OnHandleNull(float dt) { }
        public virtual void OnHandleSpawning(float dt) { }
        public virtual void OnHandleRoam(float dt) { }
        public virtual void OnHandleAggro(float dt) { }
        public virtual void OnHandleTelegraph(float dt) { }
        public virtual void OnHandleAttack(float dt) { }
        public virtual void OnHandleCooldown(float dt) { }
        public virtual void OnHandleKnockback(float dt) { }


        void HandleNull(float dt)
        {
            OnHandleNull(dt);
        }

        void HandleSpawning(float dt)
        {
            m_spawnTimer -= dt;
            if (m_spawnTimer < 0)
            {
                state = State.Roam;
            }

            OnHandleSpawning(dt);
        }

        void HandleRoam(float dt)
        {
            if (NearestPlayerDistance < AggroRange)
            {
                state = State.Aggro;
            }

            OnHandleRoam(dt);
        }

        void HandleAggro(float dt)
        {
            if (NearestPlayerDistance > BreakAggroRange)
            {
                state = State.Roam;
            }

            if (NearestPlayerDistance < AttackRange)
            {
                state = State.Telegraph;
                m_telegraphTimer = TelegraphDuration;
            }

            OnHandleAggro(dt);
        }

        void HandleTelegraph(float dt)
        {
            m_telegraphTimer -= dt;
            if (m_telegraphTimer < 0)
            {
                state = State.Attack;
                m_attackTimer = AttackDuration;
            }

            OnHandleTelegraph(dt);
        }

        void HandleAttack(float dt)
        {
            m_attackTimer -= dt;
            if (m_attackTimer < 0)
            {
                state = State.Cooldown;
                m_cooldownTimer = CooldownDuration;
            }

            OnHandleAttack(dt);
        }

        void HandleCooldown(float dt)
        {
            m_cooldownTimer -= dt;
            if (m_cooldownTimer < 0)
            {
                state = State.Roam;
            }

            OnHandleCooldown(dt);
        }

        void HandleKnockback(float dt)
        {

        }
    }
}
