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
        ClosedCollider.gameObject.SetActive(State.Value == DoorState.Closed);
        OpenCollider.gameObject.SetActive(State.Value == DoorState.Open);
    }

    public void Open()
    {
        m_animator.Play("ApeDoor_Open");
        State.Value = DoorState.Open;
        UpdateColliders();
    }

    //public void ButtonPressedDown()
    //{
    //    if (!IsServer) return;

    //    var no_buttons = new List<ApeDoorButton>(GetComponentsInChildren<ApeDoorButton>());

    //    // count our down buttons
    //    int pressedDownCount = 0;
    //    foreach (var no_button in no_buttons)
    //    {
    //        if (no_button.State.Value != ButtonState.Up) pressedDownCount++;
    //    }

    //    // if all our buttons are pressed, raise the floor and lock the buttons
    //    if (pressedDownCount >= NumberButtons)
    //    {


    //        // set all buttons to down locked
    //        foreach (var no_button in no_buttons)
    //        {
    //            no_button.State.Value = ButtonState.DownLocked;
    //        }
    //    }

    //    // ask the level parent to pop up all other platform buttons except ours
    //    PopupAllOtherApeDoorButtons();
    //}

    //private void PopupAllOtherApeDoorButtons()
    //{
    //    var no_apeDoors = transform.parent.gameObject.GetComponentsInChildren<ApeDoor>();
    //    foreach (var no_apeDoor in no_apeDoors)
    //    {
    //        if (no_apeDoor.GetComponent<NetworkObject>().NetworkObjectId != NetworkObjectId)
    //        {
    //            var no_buttons = no_apeDoor.GetComponentsInChildren<ApeDoorButton>();
    //            foreach (var no_button in no_buttons)
    //            {
    //                if (no_button.State.Value != ButtonState.DownLocked)
    //                {
    //                    no_button.SetTypeAndState(no_apeDoor.Type.Value, ButtonState.Up);
    //                }
    //            }
    //        }
    //    }
    //}

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