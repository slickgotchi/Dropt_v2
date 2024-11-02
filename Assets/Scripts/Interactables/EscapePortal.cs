using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;



public class EscapePortal : Interactable
{
    // to ensure players don't "escape" multiple times for more points
    private List<ulong> escapedNetworkObjectIds = new List<ulong>();

    public override void OnHoldFinishInteraction()
    {
        // ask server if our interacting player can escape
        TryEscapeServerRpc(playerNetworkObjectId);
    }

    [Rpc(SendTo.Server)]
    void TryEscapeServerRpc(ulong playerNetworkObjectId)
    {
        // check valid escape interaction
        if (!IsValidInteraction(playerNetworkObjectId)) return;

        // checkt his network object id has not already escaped
        if (escapedNetworkObjectIds.Contains(playerNetworkObjectId)) return;

        // do database saves
        var playerNetworkObject = NetworkManager.SpawnManager.SpawnedObjects[playerNetworkObjectId];
        if (playerNetworkObject == null) { Debug.LogWarning("Invalid player network object id tried to escape"); return; }

        var playerOffchainData = playerNetworkObject.GetComponent<PlayerOffchainData>();
        if (playerOffchainData == null) { Debug.LogWarning("Player network object id does not have PlayerOffchainData"); return; }

        // do dungeon exit calcs with an escaped = true status
        playerOffchainData.ExitDungeonCalculateBalances(true);

        // add id to escaped ids
        escapedNetworkObjectIds.Add(playerNetworkObjectId);

        // confirm with client they can escape
        EscapeConfirmedClientRpc(playerNetworkObjectId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void EscapeConfirmedClientRpc(ulong playerNetworkObjectId)
    {
        // check local player prediction matches the playerNetworkObjectId
        if (!IsLocalPlayerNetworkObjectId(playerNetworkObjectId)) return;

        // show the game over canvas
        REKTCanvas.Instance.Show(REKTCanvas.TypeOfREKT.Escaped);

        // shutdown the networkmanager for the client
        NetworkManager.Singleton.Shutdown();
    }
}
