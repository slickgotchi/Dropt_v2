using UnityEngine;
using Unity.Mathematics;
using Unity.Netcode;

// Teleport Mechanics
// - simple NetworkTransform synchorisation WITHOUT interpolation
// - server sets new teleport position at midpoint of Telegraph state
// - no stun or knockback on FudSpirits as it messes with their teleports significantly

namespace Dropt
{
    public class EnemyAI_FudSpirit : EnemyAI
    {
        [Header("FudSpirit Specific")]

        [Tooltip("This occurs withing= the starting portion of TelegraphDuration")]
        public float InvisibleDuration = 0.3f;
        public float MaxPursueRange = 16f;

        private Animator m_animator;
        private EnemyController m_enemyController;

        public enum TelegraphState { Null, FadingOut, Invisible, FadingIn }
        private TelegraphState telegraphState = TelegraphState.Null;
        private float m_fudSpiritTelegraphTimer = 1f;

        [SerializeField] private float m_fadeinDuration;
        [SerializeField] private float m_fadeoutDuration;

        private SoundFX_FudSpirit m_soundFX_FudSpirit;

        private LayerMask m_staticObstacleLayerMask;
        private LayerMask m_environmentWallLayerMask;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            m_enemyController = GetComponent<EnemyController>();
            m_soundFX_FudSpirit = GetComponent<SoundFX_FudSpirit>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            TelegraphDuration = m_fadeoutDuration + InvisibleDuration + m_fadeinDuration;

            m_staticObstacleLayerMask = LayerMask.GetMask("EnvironmentWall", "EnvironmentWater", "Destructible");
            m_environmentWallLayerMask = LayerMask.GetMask("EnvironmentWall");
        }

        public override void OnSpawnStart()
        {
            if (IsServer)
            {
                Utils.Anim.PlayAnimationWithDuration(m_animator, "FudSpirit_Fadein", SpawnDuration);
            }
            m_soundFX_FudSpirit.PlayFadeInSound();
            base.OnSpawnStart();
        }

        public override void OnAggroStart()
        {
            base.OnAggroStart();
            ChangeState(State.Telegraph);
        }

        public override void OnTelegraphStart()
        {
            base.OnTelegraphStart();

            if (!IsServer) return;

            telegraphState = TelegraphState.FadingOut;
            m_fudSpiritTelegraphTimer = m_fadeoutDuration;

            if (m_animator != null) Utils.Anim.PlayAnimationWithDuration(m_animator, "FudSpirit_Fadeout", m_fadeoutDuration);
            if (m_soundFX_FudSpirit != null) m_soundFX_FudSpirit.PlayFadeOutSound();
        }

        public override void OnTelegraphUpdate(float dt)
        {
            base.OnTelegraphUpdate(dt);

            if (!IsServer) return;

            m_fudSpiritTelegraphTimer -= dt;

            switch (telegraphState)
            {
                case TelegraphState.Null:
                    break;
                case TelegraphState.FadingOut:
                    if (m_fudSpiritTelegraphTimer < 0)
                    {
                        telegraphState = TelegraphState.Invisible;
                        m_fudSpiritTelegraphTimer = InvisibleDuration;

                        SetSpritesAndCollidersEnabled(false);
                        if (!TryTeleportToNewAttackPosition())
                        {
                            ChangeState(State.Roam);
                        }
                    }
                    break;
                case TelegraphState.Invisible:
                    if (m_fudSpiritTelegraphTimer < 0)
                    {
                        telegraphState = TelegraphState.FadingIn;
                        m_fudSpiritTelegraphTimer = m_fadeinDuration;

                        SetSpritesAndCollidersEnabled(true);
                        if (m_enemyController != null) m_enemyController.SetFacingFromDirection(AttackDirection, AttackDuration);
                        if (m_animator != null) Utils.Anim.PlayAnimationWithDuration(m_animator, "FudSpirit_Fadein", m_fadeoutDuration);
                        if (m_soundFX_FudSpirit != null) m_soundFX_FudSpirit.PlayFadeInSound();
                    }
                    break;
                case TelegraphState.FadingIn:
                    if (m_fudSpiritTelegraphTimer < 0)
                    {
                        telegraphState = TelegraphState.Null;
                    }
                    break;
                default:
                    break;
            }
        }

        public override void OnAttackStart()
        {
            // set facing
            if (!IsServer) return;
            m_enemyController.SetFacingFromDirection(AttackDirection, AttackDuration);
            Utils.Anim.Play(m_animator, "FudSpirit_Shot");
            m_soundFX_FudSpirit.PlayShootSound();
            SimpleAttackStart();
        }

        // fud spirit teleport
        private bool TryTeleportToNewAttackPosition()
        {
            if (NearestPlayer == null) return false;

            //Debug.Log("TELEPORT ENEMY " + Time.time);
            // first check if we are outside of our max range
            float dist = math.distance(transform.position, RoamAnchorPoint);
            if (dist > MaxPursueRange)
            {
                transform.position = RoamAnchorPoint;
                return false;
            }

            var nearestPlayerPosition = NearestPlayer.transform.position;

            // try teleport around player
            int maxTeleportAttempts = 10;

            for (int i = 0; i < maxTeleportAttempts; i++)
            {
                Vector3 newTeleportPosition = Utils.Battle.GetRandomSurroundPosition(
                    nearestPlayerPosition, 0.8f * AttackRange, 0.95f * AttackRange);

                // we need to check several things when teleporting to a new postion

                // 1. Do we collcide with any static obstacles?
                bool isCollingAnyObstacle = Utils.Battle.CheckCircleCollision(newTeleportPosition, 1f, m_staticObstacleLayerMask);
                if (isCollingAnyObstacle) continue;

                // 2. Can we see the player from our new position (i.e. raycast should not collide with EnvironmentWall
                var direction = nearestPlayerPosition - newTeleportPosition;
                float distance = math.distance(nearestPlayerPosition, newTeleportPosition);
                bool isHitEnvironmentWall = Physics2D.Raycast(newTeleportPosition, direction, distance, m_environmentWallLayerMask);
                if (isHitEnvironmentWall) continue;

                // if we got here a valid position has been found, lets use it
                transform.position = newTeleportPosition;
                m_enemyController.SetFacingFromDirection((NearestPlayer.transform.position - newTeleportPosition), 0.2f);
                return true;
            }

            return false;
        }

        public override void OnLateUpdate(float dt)
        {
            base.OnLateUpdate(dt);
        }

        public override void OnKnockbackStart()
        {
            base.OnKnockbackStart();

        }

        private void SetSpritesAndCollidersEnabled(bool isEnabled)
        {
            // client or host
            if (IsClient || IsHost)
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

                var canvases = GetComponentsInChildren<Canvas>();
                foreach (var canvas in canvases)
                {
                    canvas.enabled = isEnabled;
                }
            }
            // server
            else
            {
                SetSpritesAndCollidersEnabledClientRpc(isEnabled);
            }
        }

        [ClientRpc]
        private void SetSpritesAndCollidersEnabledClientRpc(bool isEnabled)
        {
            SetSpritesAndCollidersEnabled(isEnabled);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            CancelInvoke();
        }
    }
}
