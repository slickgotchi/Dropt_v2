using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class DoorButton<T> : NetworkBehaviour where T : Enum
{
    [Header("State")]
    public NetworkVariable<T> DoorType;
    public NetworkVariable<ButtonState> State;
    public int spawnerId = -1;

    public T initType;

    protected SpriteRenderer m_spriteRenderer;

    public virtual void Awake()
    {
        //Debug.Log("AWAKE BUTTON");
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        State = new NetworkVariable<ButtonState>(ButtonState.Up);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        State.OnValueChanged += OnButtonStateChange;

        DoorManager<T>.Instance.RegisterButton(this);

        if (IsServer)
        {
            DoorType.Value = initType;
        }
    }

    public override void OnNetworkDespawn()
    {
        DoorManager<T>.Instance.UnregisterButton(this);

        State.OnValueChanged -= OnButtonStateChange;

        base.OnNetworkDespawn();
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

        // try open door
        TryOpenDoor();
    }

    public void PopupAllOtherButtons()
    {
        DoorButton<T>[] allButtons = GetAllOtherDoorButtons();
        for (int i = 0; i < allButtons.Length; i++)
        {
            DoorButton<T> button = allButtons[i];
            if (button.State.Value == ButtonState.DownLocked)
            {
                continue;
            }

            if (button.spawnerId == spawnerId)
            {
                continue;
            }

            // pop up the button
            button.State.Value = ButtonState.Up;
        }
    }

    public void TryOpenDoor()
    {
        List<DoorButton<T>> matchingButtons = new List<DoorButton<T>>();
        DoorButton<T>[] allButtons = GetAllOtherDoorButtons();
        bool isAllButtonsDown = true;
        foreach (DoorButton<T> btn in allButtons)
        {
            if (btn.spawnerId != spawnerId) continue;

            matchingButtons.Add(btn);
            if (btn.State.Value == ButtonState.Up) isAllButtonsDown = false;
        }

        Door<T>[] allDoors = GetAllOtherDoor();
        Door<T> matchingDoor = null;

        foreach (Door<T> dr in allDoors)
        {
            if (dr.spawnerId == spawnerId) matchingDoor = dr;
        }

        if (matchingButtons.Count > 0 && matchingDoor != null && isAllButtonsDown)
        {
            matchingDoor.State.Value = DoorState.Open;
            //matchingDoor.Open();

            // lock down all the buttons
            foreach (DoorButton<T> btn in matchingButtons)
            {
                btn.State.Value = ButtonState.DownLocked;
            }
        }
    }

    private void Update()
    {
        //Debug.Log("DoorType -> " + DoorType.Value);
        if (IsClient)
            UpdateSprite();
    }

    public abstract void UpdateSprite();

    public abstract DoorButton<T>[] GetAllOtherDoorButtons();

    public abstract Door<T>[] GetAllOtherDoor();
}