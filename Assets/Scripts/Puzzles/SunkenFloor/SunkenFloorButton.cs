using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class SunkenFloorButton : NetworkBehaviour
{
    [Header("State")]
    public NetworkVariable<SunkenFloorType> Type;
    public NetworkVariable<ButtonState> State;
    public int spawnerId = -1;

    public SunkenFloorType initType;

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

        // try raise platform
        TryRaisePlatform();
    }

    void PopupAllOtherButtons()
    {
        var allButtons = FindObjectsByType<SunkenFloorButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < allButtons.Length; i++)
        {
            var button = allButtons[i];
            if (button.State.Value == ButtonState.DownLocked) continue;
            if (button.spawnerId == spawnerId) continue;

            // pop up the button
            button.State.Value = ButtonState.Up;
        }
    }

    void TryRaisePlatform()
    {
        var matchingButtons = new List<SunkenFloorButton>();
        var allButtons = FindObjectsByType<SunkenFloorButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        bool isAllButtonsDown = true;
        foreach (var btn in allButtons)
        {
            if (btn.spawnerId == spawnerId)
            {
                matchingButtons.Add(btn);
                if (btn.State.Value == ButtonState.Up) isAllButtonsDown = false;
            }
        }

        var allPlatforms = FindObjectsByType<SunkenFloor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        SunkenFloor matchingPlatform = null;
        foreach (var floor in allPlatforms)
        {
            if (floor.spawnerId == spawnerId) matchingPlatform = floor;
        }

        if (matchingButtons.Count > 0 && matchingPlatform != null && isAllButtonsDown)
        {
            matchingPlatform.State.Value = SunkenFloorState.Raised;
            //matchingPlatform.Raise();

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