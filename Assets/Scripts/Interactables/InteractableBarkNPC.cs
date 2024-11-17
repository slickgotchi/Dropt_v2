using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem.Wrappers;

public class InteractableBarkNPC : Interactable
{
    private bool m_isBarkOpen;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_isBarkOpen = false;
    }

    public override void OnInteractPress()
    {
        base.OnInteractPress();

        if (m_isBarkOpen)
        {
            GetComponentInChildren<StandardBarkUI>().Hide();
        }
        else
        {
            GetComponent<DialogueSystemTrigger>().Fire(transform);
        }

        m_isBarkOpen = !m_isBarkOpen;
    }

    public override void OnTriggerExit2DInteraction()
    {
        base.OnTriggerExit2DInteraction();

        GetComponentInChildren<StandardBarkUI>().Hide();
    }
}
