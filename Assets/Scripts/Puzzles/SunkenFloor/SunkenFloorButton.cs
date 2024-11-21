using UnityEngine;
using Unity.Netcode;

public class SunkenFloorButton : PlatformButton<SunkenFloorType>
{
    [Header("Sprites")]
    public Sprite DropletUp;
    public Sprite DropletDown;

    public Sprite SwirlUp;
    public Sprite SwirlDown;

    public Sprite ShroomUp;
    public Sprite ShroomDown;

    public Sprite BananasUp;
    public Sprite BananasDown;

    public Sprite GillsUp;
    public Sprite GillsDown;

    public override void Awake()
    {
        base.Awake();
        Type = new NetworkVariable<SunkenFloorType>(SunkenFloorType.Droplet);
    }

    public override void UpdateSprite()
    {
        if (State.Value == ButtonState.Up)
        {
            switch (Type.Value)
            {
                case SunkenFloorType.Droplet: m_spriteRenderer.sprite = DropletUp; break;
                case SunkenFloorType.Swirl: m_spriteRenderer.sprite = SwirlUp; break;
                case SunkenFloorType.Shroom: m_spriteRenderer.sprite = ShroomUp; break;
                case SunkenFloorType.Bananas: m_spriteRenderer.sprite = BananasUp; break;
                case SunkenFloorType.Gills: m_spriteRenderer.sprite = GillsUp; break;
                default: break;
            }
        }
        else
        {
            switch (Type.Value)
            {
                case SunkenFloorType.Droplet: m_spriteRenderer.sprite = DropletDown; break;
                case SunkenFloorType.Swirl: m_spriteRenderer.sprite = SwirlDown; break;
                case SunkenFloorType.Shroom: m_spriteRenderer.sprite = ShroomDown; break;
                case SunkenFloorType.Bananas: m_spriteRenderer.sprite = BananasDown; break;
                case SunkenFloorType.Gills: m_spriteRenderer.sprite = GillsDown; break;
                default: break;
            }
        }
    }
}