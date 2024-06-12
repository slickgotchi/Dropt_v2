using UnityEngine;
using Unity.Netcode;

public class SunkenFloorButton : NetworkBehaviour
{
    [Header("State")]
    public NetworkVariable<SunkenFloorType> Type;
    public NetworkVariable<ButtonState> State;

    [Header("Sprites")]
    public Sprite DropletUp;
    public Sprite DropletDown;

    public Sprite SwirlUp;
    public Sprite SwirlDown;

    public Sprite ShroomUp;
    public Sprite ShroomDown;

    public Sprite BananasUp;
    public Sprite BananasDown;

    public Sprite GillsUp;
    public Sprite GillsDown;

    private SpriteRenderer m_spriteRenderer;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        State = new NetworkVariable<ButtonState>(ButtonState.Up);
        Type = new NetworkVariable<SunkenFloorType>(SunkenFloorType.Droplet);
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
        var parentSunkenFloor = transform.parent.gameObject.GetComponent<SunkenFloor>();
        if (parentSunkenFloor != null)
        {
            parentSunkenFloor.ButtonPressedDown();
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

    public void SetTypeAndState(SunkenFloorType type, ButtonState state)
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
                case SunkenFloorType.Droplet: m_spriteRenderer.sprite = DropletUp; break;
                case SunkenFloorType.Swirl: m_spriteRenderer.sprite = SwirlUp; break;
                case SunkenFloorType.Shroom: m_spriteRenderer.sprite = ShroomUp; break;
                case SunkenFloorType.Bananas: m_spriteRenderer.sprite = BananasUp; break;
                case SunkenFloorType.Gills: m_spriteRenderer.sprite = GillsUp; break;
                default: break;
            }
        }
        else
        {
            switch (Type.Value)
            {
                case SunkenFloorType.Droplet: m_spriteRenderer.sprite = DropletDown; break;
                case SunkenFloorType.Swirl: m_spriteRenderer.sprite = SwirlDown; break;
                case SunkenFloorType.Shroom: m_spriteRenderer.sprite = ShroomDown; break;
                case SunkenFloorType.Bananas: m_spriteRenderer.sprite = BananasDown; break;
                case SunkenFloorType.Gills: m_spriteRenderer.sprite = GillsDown; break;
                default: break;
            }
        }
    }
}

public enum ButtonState
{
    Up, Down, DownLocked,
}