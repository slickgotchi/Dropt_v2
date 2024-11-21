using Unity.Netcode;
using UnityEngine;

public class CrystalPlatformButton : PlatformButton<CrystalPlatformType>
{
    [Header("Sprites")]
    [SerializeField] private Sprite m_triangle_Down;
    [SerializeField] private Sprite m_triangle_Up;

    [SerializeField] private Sprite m_bow_Down;
    [SerializeField] private Sprite m_bow_Up;

    [SerializeField] private Sprite m_snake_Down;
    [SerializeField] private Sprite m_snake_Up;

    [SerializeField] private Sprite m_bird_Down;
    [SerializeField] private Sprite m_bird_Up;

    [SerializeField] private Sprite m_crab_Down;
    [SerializeField] private Sprite m_crab_Up;

    public override void Awake()
    {
        base.Awake();
        Type = new NetworkVariable<CrystalPlatformType>(CrystalPlatformType.Triangle);
    }

    public override void UpdateSprite()
    {
        if (State.Value == ButtonState.Up)
        {
            switch (Type.Value)
            {
                case CrystalPlatformType.Triangle: m_spriteRenderer.sprite = m_triangle_Up; break;
                case CrystalPlatformType.Bow: m_spriteRenderer.sprite = m_bow_Up; break;
                case CrystalPlatformType.Snake: m_spriteRenderer.sprite = m_snake_Up; break;
                case CrystalPlatformType.Bird: m_spriteRenderer.sprite = m_bird_Up; break;
                case CrystalPlatformType.Crab: m_spriteRenderer.sprite = m_crab_Up; break;
                default: break;
            }
        }
        else
        {
            switch (Type.Value)
            {
                case CrystalPlatformType.Triangle: m_spriteRenderer.sprite = m_triangle_Down; break;
                case CrystalPlatformType.Bow: m_spriteRenderer.sprite = m_bow_Down; break;
                case CrystalPlatformType.Snake: m_spriteRenderer.sprite = m_snake_Down; break;
                case CrystalPlatformType.Bird: m_spriteRenderer.sprite = m_bird_Down; break;
                case CrystalPlatformType.Crab: m_spriteRenderer.sprite = m_crab_Down; break;
                default: break;
            }
        }
    }
}
