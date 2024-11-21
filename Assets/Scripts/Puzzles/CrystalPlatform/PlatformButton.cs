using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class PlatformButton<T> : NetworkBehaviour where T : Enum
{
    [Header("State")]
    public NetworkVariable<T> Type;
    public NetworkVariable<ButtonState> State;
    public int spawnerId = -1;

    public T initType;

    protected SpriteRenderer m_spriteRenderer;

    public virtual void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        State = new NetworkVariable<ButtonState>(ButtonState.Up);
        UpdateSprite();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;

        Type.Value = initType;
        State.OnValueChanged += OnButtonStateChange;
    }

    private void OnButtonStateChange(ButtonState previousValue, ButtonState newValue)
    {
        UpdateSprite();
    }

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

    private void PopupAllOtherButtons()
    {
        PlatformButton<T>[] allButtons = FindObjectsByType<PlatformButton<T>>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < allButtons.Length; i++)
        {
            PlatformButton<T> button = allButtons[i];
            if (button.State.Value == ButtonState.DownLocked) continue;
            if (button.spawnerId == spawnerId) continue;

            // pop up the button
            button.State.Value = ButtonState.Up;
        }
    }

    private void TryRaisePlatform()
    {
        List<PlatformButton<T>> matchingButtons = new List<PlatformButton<T>>();
        PlatformButton<T>[] allButtons = FindObjectsByType<PlatformButton<T>>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        bool isAllButtonsDown = true;
        foreach (PlatformButton<T> btn in allButtons)
        {
            if (btn.spawnerId == spawnerId)
            {
                matchingButtons.Add(btn);
                if (btn.State.Value == ButtonState.Up) isAllButtonsDown = false;
            }
        }

        Platform<T>[] allPlatforms = FindObjectsByType<Platform<T>>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Platform<T> matchingPlatform = null;
        foreach (Platform<T> floor in allPlatforms)
        {
            if (floor.spawnerId == spawnerId) matchingPlatform = floor;
        }

        if (matchingButtons.Count > 0 && matchingPlatform != null && isAllButtonsDown)
        {
            matchingPlatform.State.Value = PlatformState.Raised;
            //matchingPlatform.Raise();

            // lock down all the buttons
            foreach (PlatformButton<T> btn in matchingButtons)
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

    public abstract void UpdateSprite();

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        State.OnValueChanged -= OnButtonStateChange;
    }
}