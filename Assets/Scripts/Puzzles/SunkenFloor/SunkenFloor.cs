using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;
using System.Collections.Generic;

public class SunkenFloor : NetworkBehaviour
{
    [Header("State")]
    public NetworkVariable<SunkenFloorType> Type;
    public NetworkVariable<SunkenFloorState> State;
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

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        Type = new NetworkVariable<SunkenFloorType>(SunkenFloorType.Droplet);
        State = new NetworkVariable<SunkenFloorState>(SunkenFloorState.Lowered);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        SetTypeAndState(Type.Value, State.Value);
    }

    private void Update()
    {
        if (IsClient)
        {
            UpdateSprite();
            UpdateColliders();
        }
    }

    void UpdateColliders()
    {
        SunkenCollider.gameObject.SetActive(State.Value == SunkenFloorState.Lowered);
        RaisedCollider.gameObject.SetActive(State.Value == SunkenFloorState.Raised);
    }

    public void ButtonPressedDown()
    {
        if (!IsServer) return;

        var no_buttons = new List<SunkenFloorButton>(GetComponentsInChildren<SunkenFloorButton>());

        // count our down buttons
        int pressedDownCount = 0;
        foreach (var no_button in no_buttons)
        {
            if (no_button.State.Value != ButtonState.Up) pressedDownCount++;
        }

        // if all our buttons are pressed, raise the floor and lock the buttons
        if (pressedDownCount >= NumberButtons)
        {
            m_animator.Play("SunkenFloor3x3_Raise");
            State.Value = SunkenFloorState.Raised;
            UpdateColliders();

            // set all buttons to down locked
            foreach (var no_button in no_buttons)
            {
                no_button.State.Value = ButtonState.DownLocked;
            }
        }

        // ask the level parent to pop up all other platform buttons except ours
        PopupAllOtherPlatformButtons();
    }

    private void PopupAllOtherPlatformButtons()
    {
        var no_networkLevel = transform.parent.gameObject;

        var no_sunkenFloors = no_networkLevel.GetComponentsInChildren<SunkenFloor>();
        foreach (var no_sunkenFloor in no_sunkenFloors)
        {
            if (no_sunkenFloor.GetComponent<NetworkObject>().NetworkObjectId != NetworkObjectId)
            {
                var no_buttons = no_sunkenFloor.GetComponentsInChildren<SunkenFloorButton>();
                foreach (var no_button in no_buttons)
                {
                    if (no_button.State.Value != ButtonState.DownLocked)
                    {
                        no_button.SetTypeAndState(no_sunkenFloor.Type.Value, ButtonState.Up);
                    }
                }
            }
        }
    }

    public void SetTypeAndState(SunkenFloorType sunkenFloorType, SunkenFloorState floorState)
    {
        if (!IsServer) return;

        Type.Value = sunkenFloorType;
        State.Value = floorState;

        UpdateColliders();
    }

    void UpdateSprite()
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

public enum SunkenFloorType
{
    Droplet, Swirl, Shroom, Bananas, Gills,
}

public enum SunkenFloorState
{
    Lowered, Raised,
}