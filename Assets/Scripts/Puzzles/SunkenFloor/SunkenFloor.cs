using UnityEngine;
using Unity.Netcode;

public class SunkenFloor : Platform<SunkenFloorType>
{
    [Header("Emblems")]
    public SpriteRenderer RaisedEmblem;
    public SpriteRenderer SunkenEmblem;

    [Header("Sprites")]
    public Sprite DropletSunken;
    public Sprite DropletRaised;

    public Sprite SwirlSunken;
    public Sprite SwirlRaised;

    public Sprite ShroomSunken;
    public Sprite ShroomRaised;

    public Sprite BananasSunken;
    public Sprite BananasRaised;

    public Sprite GillsSunken;
    public Sprite GillsRaised;

    private Animator m_animator;

    public override void Awake()
    {
        base.Awake();
        m_animator = GetComponent<Animator>();
        Type = new NetworkVariable<SunkenFloorType>(SunkenFloorType.Droplet);
    }

    public override void PlatformRaiseAnimation()
    {
        m_animator.Play("SunkenFloor3x3_Raise");
    }

    public override void Update()
    {
        base.Update();
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        switch (Type.Value)
        {
            case SunkenFloorType.Droplet:
                SunkenEmblem.sprite = DropletSunken;
                RaisedEmblem.sprite = DropletRaised;
                break;
            case SunkenFloorType.Swirl:
                SunkenEmblem.sprite = SwirlSunken;
                RaisedEmblem.sprite = SwirlRaised;
                break;
            case SunkenFloorType.Shroom:
                SunkenEmblem.sprite = ShroomSunken;
                RaisedEmblem.sprite = ShroomRaised;
                break;
            case SunkenFloorType.Bananas:
                SunkenEmblem.sprite = BananasSunken;
                RaisedEmblem.sprite = BananasRaised;
                break;
            case SunkenFloorType.Gills:
                SunkenEmblem.sprite = GillsSunken;
                RaisedEmblem.sprite = GillsRaised;
                break;
            default: break;
        }
    }
}