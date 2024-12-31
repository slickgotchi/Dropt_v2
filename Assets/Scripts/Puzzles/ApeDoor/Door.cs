using System;
using Unity.Netcode;
using UnityEngine;

public abstract class Door<T> : NetworkBehaviour where T : Enum
{
    [Header("State")]
    public NetworkVariable<T> Type;
    public NetworkVariable<DoorState> State;
    public int NumberButtons = 2;
    public int spawnerId = -1;

    public T initType;
    public DoorState initState;

    [Header("Access")]
    [SerializeField] private Collider2D ClosedCollider;
    [SerializeField] private Collider2D OpenCollider;

    private DoorState m_localDoorState = DoorState.Closed;

    public virtual void Awake()
    {
        State = new NetworkVariable<DoorState>(DoorState.Closed);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        DoorManager<T>.Instance.RegisterDoor(this);

        if (IsServer)
        {
            SetTypeAndState(initType, initState);
        }
    }

    public override void OnNetworkDespawn()
    {
        DoorManager<T>.Instance.UnregisterDoor(this);

        base.OnNetworkDespawn();
    }

    public void Open()
    {
        if (!IsServer) return;

        State.Value = DoorState.Open;
    }

    public void SetTypeAndState(T doorType, DoorState doorState)
    {
        if (!IsServer) return;

        Type.Value = doorType;
        State.Value = doorState;

        UpdateColliders();
    }

    private void UpdateColliders()
    {
        ClosedCollider.gameObject.SetActive(State.Value == DoorState.Closed);
        OpenCollider.gameObject.SetActive(State.Value == DoorState.Open);
    }

    public virtual void Update()
    {
        if (IsClient)
        {
            // check if door is open
            if (m_localDoorState == DoorState.Closed && State.Value == DoorState.Open)
            {
                //m_animator.Play("ApeDoor_Open");
                OpenDoorAnimation();
                m_localDoorState = State.Value;
            }
        }

        UpdateColliders();
    }

    public abstract void OpenDoorAnimation();
}
