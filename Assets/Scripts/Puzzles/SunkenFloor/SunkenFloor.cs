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
    public int spawnerId = -1;

    public SunkenFloorType initType;
    public SunkenFloorState initState;

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
    [SerializeField] private GameObject SunkenCollider;
    [SerializeField] private GameObject RaisedCollider;

    private SunkenFloorState m_localSunkenFloorState = SunkenFloorState.Lowered;

    private Animator m_animator;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        Type = new NetworkVariable<SunkenFloorType>(SunkenFloorType.Droplet);
        State = new NetworkVariable<SunkenFloorState>(SunkenFloorState.Lowered);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            SetTypeAndState(initType, initState);
        }

    }

    private void Update()
    {
        if (IsClient)
        {
            // check if door is open
            if (m_localSunkenFloorState == SunkenFloorState.Lowered &&
                State.Value == SunkenFloorState.Raised)
            {
                m_animator.Play("SunkenFloor3x3_Raise");
                m_localSunkenFloorState = State.Value;
            }

            UpdateSprite();
        }

        UpdateColliders();
    }

    void UpdateColliders()
    {
        SunkenCollider.SetActive(State.Value == SunkenFloorState.Lowered);
        RaisedCollider.SetActive(State.Value == SunkenFloorState.Raised);
    }

    public void Raise()
    {
        if (!IsServer) return;

        State.Value = SunkenFloorState.Raised;
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