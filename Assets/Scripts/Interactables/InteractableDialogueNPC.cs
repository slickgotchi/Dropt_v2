using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
//using PixelCrushers.DialogueSystem.Wrappers;

public class InteractableDialogueNPC : Interactable
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        DialogueManager.instance.conversationEnded += OnConversationEnded;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        DialogueManager.instance.conversationEnded -= OnConversationEnded;
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DialogueManager.StopAllConversations();
        }
    }

    // proximity triggers
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

    // on interact
    public override void OnInteractPress()
    {
        base.OnInteractPress();

        if (!DialogueManager.isConversationActive)
        {
            GetComponent<DialogueSystemTrigger>().Fire(transform);
            PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
        }
    }

    private void OnConversationEnded(Transform actor)
    {
        PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
    }
}
