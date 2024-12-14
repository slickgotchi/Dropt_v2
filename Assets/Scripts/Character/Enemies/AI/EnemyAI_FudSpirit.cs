using UnityEngine;
using Unity.Mathematics;
using Unity.Netcode;

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
        private float m_invisibleTimer = 0f;
        private bool m_isInvisibleUsed = true;

        [SerializeField] private float m_fadeinDuration;
        [SerializeField] private float m_fadeoutDuration;

        private SoundFX_FudSpirit m_soundFX_FudSpirit;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();
            m_enemyController = GetComponent<EnemyController>();
            m_soundFX_FudSpirit = GetComponent<SoundFX_FudSpirit>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            TelegraphDuration = math.max(TelegraphDuration, InvisibleDuration + m_fadeoutDuration + m_fadeinDuration);
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

            m_invisibleTimer = InvisibleDuration + m_fadeoutDuration;
            m_isInvisibleUsed = false;
            Utils.Anim.PlayAnimationWithDuration(m_animator, "FudSpirit_Fadeout", m_fadeoutDuration);
            m_soundFX_FudSpirit.PlayFadeOutSound();
            Invoke(nameof(DisableSpritesAndCollidersAndTeleportToNewPosition), m_fadeoutDuration);
        }

        private void DisableSpritesAndCollidersAndTeleportToNewPosition()
        {
            SetSpritesAndCollidersEnabled(false);
            TeleportToNewAttackPosition();
        }

        public override void OnTelegraphUpdate(float dt)
        {
            base.OnTelegraphUpdate(dt);
            if (!IsServer)
            {
                return;
            }

            m_invisibleTimer -= dt;

            if (m_invisibleTimer <= 0 && !m_isInvisibleUsed)
            {
                m_isInvisibleUsed = true;

                SetSpritesAndCollidersEnabled(true);
                m_enemyController.SetFacingFromDirection(AttackDirection, AttackDuration);
                Utils.Anim.PlayAnimationWithDuration(m_animator, "FudSpirit_Fadein", m_fadeoutDuration);
                m_soundFX_FudSpirit.PlayFadeInSound();
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
        private void TeleportToNewAttackPosition()
        {
            //Debug.Log("TELEPORT ENEMY " + Time.time);
            // first check if we are outside of our max range
            float dist = math.distance(transform.position, RoamAnchorPoint);
            if (dist > MaxPursueRange)
            {
                transform.position = RoamAnchorPoint;
                return;
            }

            // try teleport around player
            int maxTeleportAttempts = 10;

            for (int i = 0; i < maxTeleportAttempts; i++)
            {
                Vector3 newPosition = Utils.Battle.GetRandomSurroundPosition(
                    NearestPlayer.transform.position, 0.8f * AttackRange, 0.95f * AttackRange);

                // check for any overlaps
                // Define the LayerMask using the layers you want to check against
                LayerMask specificLayerMask = LayerMask.GetMask("EnvironmentWall",
                    "EnvironmentWater", "Destructible");
                bool isColliding = Utils.Battle.CheckCircleCollision(newPosition, 1f, specificLayerMask);
                if (isColliding)
                {
                    continue;
                }
                else
                {
                    // teleport to new position
                    if (!IsHost)
                    {
                        transform.position = newPosition;
                    }
                    TeleportClientRpc(newPosition);
                    return;
                }
            }
        }

        [ClientRpc]
        private void TeleportClientRpc(Vector3 newPosition)
        {
            transform.position = newPosition;
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
