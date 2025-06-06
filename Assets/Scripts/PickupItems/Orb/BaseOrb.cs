using UnityEngine;
using Unity.Netcode;

namespace PickupItems.Orb
{
    public abstract class BaseOrb : NetworkBehaviour
    {
        public Sprite tinySprite;
        public Sprite smallSprite;
        public Sprite mediumSprite;
        public Sprite largeSprite;

        public SpriteRenderer m_spriteRenderer;

        private int m_value = 1;

        protected PickupItemManager.Size m_size;

        public virtual void Init(PickupItemManager.Size size)
        {
            m_size = size;

            switch (size)
            {
                case PickupItemManager.Size.Tiny:
                    m_spriteRenderer.sprite = tinySprite;
                    break;
                case PickupItemManager.Size.Small:
                    m_spriteRenderer.sprite = smallSprite;
                    break;
                case PickupItemManager.Size.Medium:
                    m_spriteRenderer.sprite = mediumSprite;
                    break;
                case PickupItemManager.Size.Large:
                    m_spriteRenderer.sprite = largeSprite;
                    break;
                default: break;
            }

            m_value = CalculateValue(size);
        }

        protected abstract int CalculateValue(PickupItemManager.Size size);

        public int GetValue()
        {
            return m_value;
        }
    }
}