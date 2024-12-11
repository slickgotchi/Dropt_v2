using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;



public class EscapePortal : Interactable
{
    // to ensure players don't "escape" multiple times for more points
    private List<ulong> escapedNetworkObjectIds = new List<ulong>();

    public override void OnInteractHoldFinish()
    {
        // ask server if our interacting local player can escape
        TryEscapeServerRpc(localPlayerNetworkObjectId);
    }

    public override void OnTriggerEnter2DInteraction()
    {
        base.OnTriggerEnter2DInteraction();

        PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
    }

    public override void OnTriggerExit2DInteraction()
    {
        base.OnTriggerExit2DInteraction();

        PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
    }

    [Rpc(SendTo.Server)]
    void TryEscapeServerRpc(ulong playerNetworkObjectId)
    {
        // check valid escape interaction
        if (!IsValidInteraction(playerNetworkObjectId)) return;

        // check this network object id has not already escaped
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

        // try update leaderboard
        var playerLeaderboardLogger = playerNetworkObject.GetComponent<PlayerLeaderboardLogger>();
        if (playerLeaderboardLogger != null)
        {
            playerLeaderboardLogger.LogEndOfDungeonResults(true);
        }

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
