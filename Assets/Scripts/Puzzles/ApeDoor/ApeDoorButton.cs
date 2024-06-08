using UnityEngine;
using Unity.Netcode;

public class ApeDoorButton : NetworkBehaviour
{
    [Header("State")]
    public ApeDoorType ApeDoorType = ApeDoorType.Crescent;
    public ButtonState ButtonState = ButtonState.Up;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        SetTypeStateAndSprite(ApeDoorType, ButtonState.Up);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (ButtonState != ButtonState.Up) return;

        // update button state
        ButtonState = ButtonState.Down;
        UpdateButtonClientRpc(ButtonState);

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

    [ClientRpc]
    void UpdateButtonClientRpc(ButtonState buttonState)
    {
        SetTypeStateAndSprite(ApeDoorType, buttonState);
    }

    public void SetTypeStateAndSprite(ApeDoorType apeDoorType, ButtonState buttonState)
    {
        ApeDoorType = apeDoorType;
        ButtonState = buttonState;

        if (buttonState == ButtonState.Up)
        {
            switch (apeDoorType)
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
            switch (apeDoorType)
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