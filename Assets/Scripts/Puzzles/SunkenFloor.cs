using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;
using System.Collections.Generic;

public class SunkenFloor : NetworkBehaviour
{
    [Header("State")]
    public SunkenFloorType SunkenFloorType = SunkenFloorType.Droplet;
    public bool IsRaised = false;
    public int NumberButtons = 2;

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

    private void Start()
    {

    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        SetTypeAndSprite(SunkenFloorType);
    }

    public void SetTypeAndSprite(SunkenFloorType sunkenFloorType)
    {
        SunkenFloorType = sunkenFloorType;

        switch (sunkenFloorType)
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

public enum SunkenFloorType
{
    Droplet, Swirl, Shroom, Bananas, Gills,
}