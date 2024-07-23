using Unity.Netcode;
using UnityEngine;

namespace Level.Traps
{
    public sealed class TrapButton : Trap
    {
        [HideInInspector]
        public NetworkVariable<bool> IsActive;

        [SerializeField] private SpriteRenderer m_buttonImage;
        [SerializeField] private Sprite m_activeSprite;
        [SerializeField] private Sprite m_disabledSprite;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsServer || null == m_group)
                return;

            if (IsActive.Value)
                return;

            var player = collision.gameObject.GetComponent<PlayerController>();
            if (player == null)
                return;

            foreach (var trap in m_group.Traps)
            {
                if (!(trap is PressurePlateTrap))
                    continue;

                (trap as PressurePlateTrap).IsActive.Value = true;
            }

            m_cooldownTimer = m_cooldownDuration;
            IsActive.Value = true;
        }

        protected override void Update()
        {
            var sprite = (IsActive.Value) ? m_activeSprite : m_disabledSprite;
            if (sprite != m_buttonImage.sprite)
            {
                m_buttonImage.sprite = sprite;
            }

            if (!IsServer || null == m_group)
                return;

            if (!IsActive.Value)
                return;

            m_cooldownTimer -= Time.deltaTime;

            if (m_cooldownTimer > 0)
                return;

            foreach (var trap in m_group.Traps)
            {
                if (!(trap is PressurePlateTrap))
                    continue;

                (trap as PressurePlateTrap).IsActive.Value = false;
            }

            IsActive.Value = false;
        }
    }
}