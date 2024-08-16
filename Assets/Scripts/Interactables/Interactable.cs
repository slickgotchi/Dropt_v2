using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Interactable : NetworkBehaviour
{
    public enum Status
    {
        Inactive,
        Active,
    }
    public Status status;

    public ulong playerNetworkObjectId;

    public virtual void OnStartInteraction() { }

    public virtual void OnUpdateInteraction() { }

    public virtual void OnFinishInteraction() { }

    public bool IsPlayerIdLocal(ulong playerNetworkObjectId)
    {
        return NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId].IsLocalPlayer;
    }
}
