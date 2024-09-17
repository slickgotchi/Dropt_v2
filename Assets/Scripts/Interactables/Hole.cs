using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Hole : Interactable
{
    public List<GameObject> Levels = new List<GameObject>();

    public override void OnHoldFinishInteraction()
    {
        TryGoToNextLevelServerRpc(playerNetworkObjectId);
    }

    [Rpc(SendTo.Server)]
    void TryGoToNextLevelServerRpc(ulong testPlayerNetworkObjectId)
    {
        if (!IsValidInteraction(testPlayerNetworkObjectId)) return;

        // see if this hole has a custom levels list
        if (Levels.Count > 0)
        {
            LevelManager.Instance.SetLevelList(Levels);
        }

        // go to next level
        LevelManager.Instance.GoToNextLevel();
    }
}
