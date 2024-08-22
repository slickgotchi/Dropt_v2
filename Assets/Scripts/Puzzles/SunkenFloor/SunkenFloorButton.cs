using UnityEngine;
using Unity.Netcode;

public class SunkenFloorButton : NetworkBehaviour
{
    [Header("State")]
    public NetworkVariable<SunkenFloorType> Type;
    public NetworkVariable<ButtonState> State;
    public int spawnerId = -1;

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

    // WARNING: TriggerEnter/Exit can be somewhat flakey when combined with my PlayerMovement predictino code
    // therefore using a Physics2D.XXXXOverlap() function should usually be preferred
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (State.Value != ButtonState.Up) return;

        // update button state
        State.Value = ButtonState.Down;

        // find sunken floor button group with matchin id
        var sunkenFloorButtonGroups = FindObjectsByType<SunkenFloorButtonGroup>
            (FindObjectsInactive.Include, FindObjectsSortMode.None);
        bool isFoundButtonGroup = false;
        for (int i = 0; i < sunkenFloorButtonGroups.Length; i++)
        {
            if (sunkenFloorButtonGroups[i].spawnerId == spawnerId)
            {
                sunkenFloorButtonGroups[i].ButtonPressedDown();
                isFoundButtonGroup = true;
                break;
            }
        }

        if (!isFoundButtonGroup)
        {
            Debug.LogWarning("Warning: SunkenFloorButton spawnerId: "
                + spawnerId + ", does not have a parent SunkenFloorButtonGroup");
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