using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class ApeDoorButton : NetworkBehaviour
{
    [Header("State")]
    public NetworkVariable<ApeDoorType> Type;
    public NetworkVariable<ButtonState> State;
    public int spawnerId = -1;

    public ApeDoorType initType;

    [Header("Sprites")]
    public Sprite CrescentUp;
    public Sprite CrescentDown;

    public Sprite TriangleUp;
    public Sprite TriangleDown;

    public Sprite HexagonUp;
    public Sprite HexagonDown;

    public Sprite PlusUp;
    public Sprite PlusDown;

    public Sprite SquareUp;
    public Sprite SquareDown;

    private SpriteRenderer m_spriteRenderer;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        Type = new NetworkVariable<ApeDoorType>(ApeDoorType.Crescent);
        State = new NetworkVariable<ButtonState>(ButtonState.Up);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Type.Value = initType;
    }

    // WARNING: TriggerEnter/Exit can be somewhat flakey when combined with my PlayerMovement predictino code
    // therefore using a Physics2D.XXXXOverlap() function should usually be preferred
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (State.Value != ButtonState.Up) return;

        // update button state
        State.Value = ButtonState.Down;

        // popup all other buttons
        PopupAllOtherButtons();

        // try open door
        TryOpenDoor();
    }

    void PopupAllOtherButtons()
    {
        var allButtons = FindObjectsByType<ApeDoorButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < allButtons.Length; i++)
        {
            var button = allButtons[i];
            if (button.State.Value == ButtonState.DownLocked) continue;
            if (button.spawnerId == spawnerId) continue;

            // pop up the button
            button.State.Value = ButtonState.Up;
        }
    }

    void TryOpenDoor()
    {
        var matchingButtons = new List<ApeDoorButton>();
        var allButtons = FindObjectsByType<ApeDoorButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        bool isAllButtonsDown = true;
        foreach (var btn in allButtons)
        {
            if (btn.spawnerId == spawnerId)
            {
                matchingButtons.Add(btn);
                if (btn.State.Value == ButtonState.Up) isAllButtonsDown = false;
            }
        }

        var allDoors = FindObjectsByType<ApeDoor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        ApeDoor matchingDoor = null;
        foreach (var dr in allDoors)
        {
            if (dr.spawnerId == spawnerId) matchingDoor = dr;
        }

        if (matchingButtons.Count > 0 && matchingDoor != null && isAllButtonsDown)
        {
            matchingDoor.Open();

            // lock down all the buttons
            foreach (var btn in matchingButtons)
            {
                btn.State.Value = ButtonState.DownLocked;
            }
        }
    }

    private void Update()
    {
        if (IsClient)
        {
            UpdateSprite();
        }
    }

    void UpdateSprite()
    {
        if (State.Value == ButtonState.Up)
        {
            switch (Type.Value)
            {
                case ApeDoorType.Crescent: m_spriteRenderer.sprite = CrescentUp; break;
                case ApeDoorType.Triangle: m_spriteRenderer.sprite = TriangleUp; break;
                case ApeDoorType.Hexagon: m_spriteRenderer.sprite = HexagonUp; break;
                case ApeDoorType.Plus: m_spriteRenderer.sprite = PlusUp; break;
                case ApeDoorType.Square: m_spriteRenderer.sprite = SquareUp; break;
                default: break;
            }
        }
        else
        {
            switch (Type.Value)
            {
                case ApeDoorType.Crescent: m_spriteRenderer.sprite = CrescentDown; break;
                case ApeDoorType.Triangle: m_spriteRenderer.sprite = TriangleDown; break;
                case ApeDoorType.Hexagon: m_spriteRenderer.sprite = HexagonDown; break;
                case ApeDoorType.Plus: m_spriteRenderer.sprite = PlusDown; break;
                case ApeDoorType.Square: m_spriteRenderer.sprite = SquareDown; break;
                default: break;
            }
        }
    }

}