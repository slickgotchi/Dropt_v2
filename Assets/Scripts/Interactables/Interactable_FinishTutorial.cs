using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Interactable_FinishTutorial : Interactable
{
    public override void OnInteractHoldFinish()
    {
        if (IsClient)
        {
            // we set tutorial complete to 1 
            PlayerPrefs.SetInt("IsTutorialComplete", 1);

            TryGoToDegeneapeVillageServerRpc(localPlayerNetworkObjectId);
        }
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
    void TryGoToDegeneapeVillageServerRpc(ulong testPlayerNetworkObjectId)
    {
        if (!IsValidInteraction(testPlayerNetworkObjectId)) return;

        // go to next level
        LevelManager.Instance.GoToDegenapeVillageLevel_SERVER();

    }
}
