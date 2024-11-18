using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using PixelCrushers.DialogueSystem.Wrappers;
using PixelCrushers.DialogueSystem;

public class InteractableBarkNPC : Interactable
{
    private bool m_isBarkOpen;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_isBarkOpen = false;
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Q) && m_isBarkOpen)
        {
            GetComponentInChildren<StandardBarkUI>().Hide();
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
            m_isBarkOpen = false;
        }
    }

    public override void OnInteractPress()
    {
        base.OnInteractPress();

        if (m_isBarkOpen)
        {
            GetComponentInChildren<StandardBarkUI>().Hide();
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
        }
        else
        {
            GetComponent<DialogueSystemTrigger>().Fire(transform);
            PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
        }

        m_isBarkOpen = !m_isBarkOpen;
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

        GetComponentInChildren<StandardBarkUI>().Hide();
        PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
        m_isBarkOpen = false;
    }
}
