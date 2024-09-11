using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;
using System.Collections.Generic;

public class ApeDoor : NetworkBehaviour
{
    [Header("State")]
    public NetworkVariable<ApeDoorType> Type;
    public NetworkVariable<DoorState> State;
    public int NumberButtons = 2;
    public int spawnerId = -1;

    public ApeDoorType initType;
    public DoorState initState;

    [Header("Emblems")]
    public SpriteRenderer LeftEmblem;
    public SpriteRenderer RightEmblem;

    [Header("Sprites")]
    public Sprite CrescentLeft;
    public Sprite CrescentRight;

    public Sprite TriangleLeft;
    public Sprite TriangleRight;

    public Sprite HexagonLeft;
    public Sprite HexagonRight;

    public Sprite PlusLeft;
    public Sprite PlusRight;

    public Sprite SquareLeft;
    public Sprite SquareRight;

    [Header("Access")]
    [SerializeField] private Collider2D ClosedCollider;
    [SerializeField] private Collider2D OpenCollider;

    private Animator m_animator;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        Type = new NetworkVariable<ApeDoorType>(ApeDoorType.Crescent);
        State = new NetworkVariable<DoorState>(DoorState.Closed);
    }

    public override void OnNetworkSpawn()
    {
        SetTypeAndState(initType, initState);
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
        ClosedCollider.gameObject.SetActive(State.Value == DoorState.Closed);
        OpenCollider.gameObject.SetActive(State.Value == DoorState.Open);
    }

    public void Open()
    {
        m_animator.Play("ApeDoor_Open");
        State.Value = DoorState.Open;
        UpdateColliders();
    }

    public void SetTypeAndState(ApeDoorType apeDoorType, DoorState doorState)
    {
        if (!IsServer) return;

        Type.Value = apeDoorType;
        State.Value = doorState;

        UpdateColliders();
    }

    void UpdateSprite()
    {
        switch (Type.Value)
        {
            case ApeDoorType.Crescent:
                LeftEmblem.sprite = CrescentLeft;
                RightEmblem.sprite = CrescentRight;
                break;
            case ApeDoorType.Triangle:
                LeftEmblem.sprite = TriangleLeft;
                RightEmblem.sprite = TriangleRight;
                break;
            case ApeDoorType.Hexagon:
                LeftEmblem.sprite = HexagonLeft;
                RightEmblem.sprite = HexagonRight;
                break;
            case ApeDoorType.Plus:
                LeftEmblem.sprite = PlusLeft;
                RightEmblem.sprite = PlusRight;
                break;
            case ApeDoorType.Square:
                LeftEmblem.sprite = SquareLeft;
                RightEmblem.sprite = SquareRight;
                break;
            default: break;
        }
    }
}

public enum ApeDoorType
{
    Crescent, Triangle, Hexagon, Plus, Square,
}

public enum DoorState
{
    Open, Closed,
}