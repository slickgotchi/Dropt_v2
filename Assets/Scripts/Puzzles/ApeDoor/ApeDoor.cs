using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;
using System.Collections.Generic;

public class ApeDoor : NetworkBehaviour
{
    [Header("State")]
    public ApeDoorType ApeDoorType = ApeDoorType.Crescent;
    public DoorState ApeDoorState = DoorState.Closed;
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

    private void Start()
    {
        m_animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        SetTypeAndSprite(ApeDoorType);

        ClosedCollider.gameObject.SetActive(true);
        OpenCollider.gameObject.SetActive(false);
    }

    public void ButtonPressedDown()
    {
        if (!IsServer) return;

        var no_buttons = new List<ApeDoorButton>(GetComponentsInChildren<ApeDoorButton>());

        // count our down buttons
        int pressedDownCount = 0;
        foreach (var no_button in no_buttons)
        {
            if (no_button.ButtonState != ButtonState.Up) pressedDownCount++;
        }

        // if all our buttons are pressed, raise the floor and lock the buttons
        if (pressedDownCount >= NumberButtons)
        {
            m_animator.Play("ApeDoor_Open");
            ClosedCollider.gameObject.SetActive(false);
            OpenCollider.gameObject.SetActive(true);

            // set all buttons to down locked
            foreach (var no_button in no_buttons)
            {
                no_button.ButtonState = ButtonState.DownLocked;
            }
        }

        // ask the level parent to pop up all other platform buttons except ours
        transform.parent.gameObject.GetComponent<NetworkLevel>().PopupAllApeDoorButtonsExcept(NetworkObjectId);
    }

    public void SetTypeAndSprite(ApeDoorType apeDoorType)
    {
        ApeDoorType = apeDoorType;

        switch (apeDoorType)
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