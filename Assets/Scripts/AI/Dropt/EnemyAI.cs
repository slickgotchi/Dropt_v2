using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dropt
{
    public class EnemyAI : MonoBehaviour
    {
        public float SpawnDuration = 1f;
        public float AggroRange = 6f;
        public float PursueSpeed = 4f;
        public float RoamSpeed = 2f;
        public float AttackRange = 1.5f;
        public float TelegraphAttackDuration = 1f;
        public float BreakAggroRange = 10f;

        public GameObject Attack;

        public enum State
        {
            Null,
            Spawning,
            Idle,
            Roam,
            Aggro,
            TelegraphAttack,
            Attack,
            Knockback,
        }

        public State state = State.Idle;

        private void Update()
        {
            float dt = Time.deltaTime;

            switch (state)
            {
                case State.Null:
                    HandleNull(dt);
                    break;
                default: break;
            }
        }

        void HandleNull(float dt)
        {

        }
    }
}
