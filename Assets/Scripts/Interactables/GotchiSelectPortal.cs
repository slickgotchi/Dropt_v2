using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GotchiHub;

public class GotchiSelectPortal : Interactable
{
    public override void OnInteractPress()
    {
        base.OnInteractPress();

        // set our interactable
        GotchiSelectCanvas.Instance.interactable = GetComponent<Interactable>();

        if (GotchiSelectCanvas.Instance.isCanvasOpen)
        {
            GotchiSelectCanvas.Instance.HideCanvas();
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
        }
        else
        {
            GotchiSelectCanvas.Instance.ShowCanvas();
            PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
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
}
