using System;
using Unity.Netcode;
using UnityEngine;

public abstract class Platform<T> : NetworkBehaviour where T : Enum
{
    [Header("State")]
    public NetworkVariable<T> Type;
    public NetworkVariable<PlatformState> State;
    public int NumberButtons = 2;
    public int spawnerId = -1;

    public T initType;
    public PlatformState initState;

    [Header("Access")]
    [SerializeField] private GameObject PlatformCollider;
    [SerializeField] private GameObject RaisedCollider;

    private PlatformState m_localPlatformState = PlatformState.Lowered;

    public virtual void Awake()
    {
        State = new NetworkVariable<PlatformState>(PlatformState.Lowered);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            SetTypeAndState(initType, initState);
        }
    }

    public void SetTypeAndState(T floorType, PlatformState floorState)
    {
        if (!IsServer) return;

        Type.Value = floorType;
        State.Value = floorState;

        UpdateColliders();
    }

    private void UpdateColliders()
    {
        PlatformCollider.SetActive(State.Value == PlatformState.Lowered);
        RaisedCollider.SetActive(State.Value == PlatformState.Raised);
    }

    public void Raise()
    {
        if (!IsServer) return;

        State.Value = PlatformState.Raised;
    }

    public virtual void Update()
    {
        if (IsClient)
        {
            // check if door is open
            if (m_localPlatformState == PlatformState.Lowered &&
                State.Value == PlatformState.Raised)
            {
                PlatformRaiseAnimation();
                m_localPlatformState = State.Value;
            }
        }

        UpdateColliders();
    }

    public abstract void PlatformRaiseAnimation();
}