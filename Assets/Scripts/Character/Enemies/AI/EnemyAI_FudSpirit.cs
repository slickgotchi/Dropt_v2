using Dropt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Mathematics;

namespace Dropt
{
    public class EnemyAI_FudSpirit : EnemyAI
    {
        [Header("FudSpirit Specific")]

        [Tooltip("This occurs withing= the starting portion of TelegraphDuration")]
        public float InvisibleDuration = 0.3f;
        public float MaxPursueRange = 16f;

        private Animator m_animator;
        private float m_invisibleTimer = 0f;
        private bool m_isInvisibleUsed = true;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            TelegraphDuration = math.max(TelegraphDuration, InvisibleDuration);
        }

        public override void OnAggroStart()
        {
            base.OnAggroStart();

            // go invisible
            SetSpritesAndCollidersEnabled(false);

            // pick a new attack position
            m_invisibleTimer = InvisibleDuration;
            m_isInvisibleUsed = false;

            // teleport to a new position within attack range
            TeleportToNewAttackPosition();
        }

        public override void OnTelegraphStart()
        {

        }

        public override void OnTelegraphUpdate(float dt)
        {
            base.OnTelegraphUpdate(dt);

            m_invisibleTimer -= dt;
            if (m_invisibleTimer < 0 && !m_isInvisibleUsed)
            {
                m_isInvisibleUsed = true;
                SetSpritesAndCollidersEnabled(true);

                // teleport to player location
                TeleportToNewAttackPosition();

                // calc our attack direction
                CalculateAttackDirectionAndPosition();

                // set our facing direction
                GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, TelegraphDuration);
            }
        }

        public override void OnTelegraphFinish()
        {
            base.OnTelegraphFinish();

            // just double check everything is reenabled
            SetSpritesAndCollidersEnabled(true);
        }

        public override void OnAttackStart()
        {
            SimpleAttackStart();

            // set facing
            GetComponent<EnemyController>().SetFacingFromDirection(AttackDirection, AttackDuration);
        }


        //public override void OnKnockback(Vector3 direction, float distance, float duration)
        //{
        //    SimpleKnockback(direction, distance, duration);

        //}

        // fud spirit teleport
        private void TeleportToNewAttackPosition()
        {
            // first check if we are outside of our max range
            var dist = math.distance(transform.position, RoamAnchorPoint);
            if (dist > MaxPursueRange)
            {
                transform.position = RoamAnchorPoint;
                return;
            }

            // try teleport around player
            int maxTeleportAttempts = 10;

            for (int i = 0; i < maxTeleportAttempts; i++)
            {
                var newPosition = Dropt.Utils.Battle.GetRandomSurroundPosition(
                    NearestPlayer.transform.position, 0.8f * AttackRange, 0.95f * AttackRange);

                // check for any overlaps
                // Define the LayerMask using the layers you want to check against
                LayerMask specificLayerMask = LayerMask.GetMask("EnvironmentWall",
                    "EnvironmentWater", "Destructible");
                bool isColliding = Dropt.Utils.Battle.CheckCircleCollision(newPosition, 1f, specificLayerMask);
                if (isColliding)
                {
                    continue;
                } else
                {
                    // teleport to new position
                    transform.position = newPosition;
                    return;
                }
            }
        }

        void SetSpritesAndCollidersEnabled(bool isEnabled)
        {
            // hide the spirit and disable all its colliders
            var sprites = GetComponentsInChildren<SpriteRenderer>();
            foreach (var sprite in sprites)
            {
                sprite.enabled = isEnabled;
            }

            var colliders = GetComponentsInChildren<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.enabled = isEnabled;
            }
        }
    }
}
