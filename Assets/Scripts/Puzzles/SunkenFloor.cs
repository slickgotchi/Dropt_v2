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

    [Header("Access")]
    [SerializeField] private Collider2D SunkenCollider;
    [SerializeField] private Collider2D RaisedCollider;

    private Animator m_animator;

    private void Start()
    {
        m_animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        SetTypeAndSprite(SunkenFloorType);

        SunkenCollider.gameObject.SetActive(true);
        RaisedCollider.gameObject.SetActive(false);
    }

    public void ButtonPressedDown()
    {
        if (!IsServer) return;

        var no_buttons = new List<SunkenFloorButton>(GetComponentsInChildren<SunkenFloorButton>());

        // count our down buttons
        int pressedDownCount = 0;
        foreach (var no_button in no_buttons)
        {
            if (no_button.ButtonState != ButtonState.Up) pressedDownCount++;
        }

        // if all our buttons are pressed, raise the floor and lock the buttons
        if (pressedDownCount >= NumberButtons)
        {
            m_animator.Play("SunkenFloor3x3_Raise");
            SunkenCollider.gameObject.SetActive(false);
            RaisedCollider.gameObject.SetActive(true);

            // set all buttons to down locked
            foreach (var no_button in no_buttons)
            {
                no_button.ButtonState = ButtonState.DownLocked;
            }
        }

        // ask the level parent to pop up all other platform buttons except ours
        transform.parent.gameObject.GetComponent<NetworkLevel>().PopupAllPlatformButtonsExcept(NetworkObjectId);
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