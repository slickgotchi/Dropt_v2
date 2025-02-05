using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Hole : Interactable
{
    public List<GameObject> Levels = new List<GameObject>();

    public override void OnInteractHoldFinish()
    {
        TryGoToNextLevelServerRpc(localPlayerNetworkObjectId);
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
    void TryGoToNextLevelServerRpc(ulong testPlayerNetworkObjectId)
    {
        if (!IsValidInteraction(testPlayerNetworkObjectId)) return;

        // see if this hole has a custom levels list
        if (Levels.Count > 0)
        {
            LevelManager.Instance.SetLevelList_SERVER(Levels);
        }

        // go to next level
        LevelManager.Instance.TransitionToNextLevel_SERVER();
    }
}
