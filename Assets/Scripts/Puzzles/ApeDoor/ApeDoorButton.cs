using UnityEngine;
using Unity.Netcode;

public class ApeDoorButton : NetworkBehaviour
{
    [Header("State")]
    public NetworkVariable<ApeDoorType> Type;
    public NetworkVariable<ButtonState> State;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        SetTypeAndState(Type.Value, State.Value);
    }

    // WARNING: TriggerEnter/Exit can be somewhat flakey when combined with my PlayerMovement predictino code
    // therefore using a Physics2D.XXXXOverlap() function should usually be preferred
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (State.Value != ButtonState.Up) return;

        // update button state
        State.Value = ButtonState.Down;

        // grab the parent sunken floor and get it to check status of all its buttons
        var parentApeDoor = transform.parent.gameObject.GetComponent<ApeDoor>();
        if (parentApeDoor != null)
        {
            parentApeDoor.ButtonPressedDown();
        } else
        {
            Debug.Log("Error: SunkenFloorButton does not have a parent SunkenFloor");
        }
    }

    private void Update()
    {
        if (IsClient)
        {
            UpdateSprite();
        }
    }

    public void SetTypeAndState(ApeDoorType type, ButtonState state)
    {
        if (!IsServer) return;

        Type.Value = type;
        State.Value = state;
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