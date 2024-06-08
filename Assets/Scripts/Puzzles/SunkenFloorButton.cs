using UnityEngine;
using Unity.Netcode;

public class SunkenFloorButton : NetworkBehaviour
{
    [Header("State")]
    public SunkenFloorType SunkenFloorType = SunkenFloorType.Droplet;
    public ButtonState ButtonState = ButtonState.Up;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        SetTypeStateAndSprite(SunkenFloorType, ButtonState.Up);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (ButtonState != ButtonState.Up) return;

        // update button state
        ButtonState = ButtonState.Down;
        UpdateButtonClientRpc(ButtonState);

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

    [ClientRpc]
    void UpdateButtonClientRpc(ButtonState buttonState)
    {
        SetTypeStateAndSprite(SunkenFloorType, buttonState);
    }

    public void SetTypeStateAndSprite(SunkenFloorType sunkenFloorType, ButtonState buttonState)
    {
        SunkenFloorType = sunkenFloorType;
        ButtonState = buttonState;

        if (buttonState == ButtonState.Up)
        {
            switch (sunkenFloorType)
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
            switch (sunkenFloorType)
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