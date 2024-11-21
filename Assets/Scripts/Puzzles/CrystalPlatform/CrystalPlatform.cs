using Unity.Netcode;
using UnityEngine;
using DG.Tweening;

public class CrystalPlatform : Platform<CrystalPlatformType>
{
    [SerializeField] private SpriteRenderer m_platform;
    [SerializeField] private SpriteRenderer m_platformIcon;

    [SerializeField] private Sprite m_platform_Normal;
    [SerializeField] private Sprite m_platform_Raised;

    [SerializeField] private Sprite m_triangle_Normal;
    [SerializeField] private Sprite m_triangle_Raised;

    [SerializeField] private Sprite m_bow_Normal;
    [SerializeField] private Sprite m_bow_Raised;

    [SerializeField] private Sprite m_snake_Normal;
    [SerializeField] private Sprite m_snake_Raised;

    [SerializeField] private Sprite m_bird_Normal;
    [SerializeField] private Sprite m_bird_Raised;

    [SerializeField] private Sprite m_crab_Normal;
    [SerializeField] private Sprite m_crab_Raised;

    public override void Awake()
    {
        base.Awake();
        Type = new NetworkVariable<CrystalPlatformType>(CrystalPlatformType.Triangle);
        UpdatePlatformSprite();
    }

    public override void PlatformRaiseAnimation()
    {
        Transform platformTransform = m_platform.transform;
        _ = platformTransform.DOLocalMoveY(-0.25f, 0.15f).SetEase(Ease.Linear).OnComplete(() =>
          {
              UpdatePlatformSprite();
              _ = platformTransform.DOLocalMoveY(0, 0.15f).SetEase(Ease.Linear);
          });
    }

    public void UpdatePlatformSprite()
    {
        if (State.Value == PlatformState.Lowered)
        {
            m_platform.sprite = m_platform_Normal;
            switch (Type.Value)
            {
                case CrystalPlatformType.Triangle:
                    m_platformIcon.sprite = m_triangle_Normal;
                    break;
                case CrystalPlatformType.Bow:
                    m_platformIcon.sprite = m_bow_Normal;
                    break;
                case CrystalPlatformType.Snake:
                    m_platformIcon.sprite = m_snake_Normal;
                    break;
                case CrystalPlatformType.Bird:
                    m_platformIcon.sprite = m_bird_Normal;
                    break;
                case CrystalPlatformType.Crab:
                    m_platformIcon.sprite = m_crab_Normal;
                    break;
            }
        }
        else
        {
            m_platform.sprite = m_platform_Raised;
            switch (Type.Value)
            {
                case CrystalPlatformType.Triangle:
                    m_platformIcon.sprite = m_triangle_Raised;
                    break;
                case CrystalPlatformType.Bow:
                    m_platformIcon.sprite = m_bow_Raised;
                    break;
                case CrystalPlatformType.Snake:
                    m_platformIcon.sprite = m_snake_Raised;
                    break;
                case CrystalPlatformType.Bird:
                    m_platformIcon.sprite = m_bird_Raised;
                    break;
                case CrystalPlatformType.Crab:
                    m_platformIcon.sprite = m_crab_Raised;
                    break;
            }
        }
    }
}