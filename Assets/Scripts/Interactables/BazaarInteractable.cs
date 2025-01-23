using UnityEngine;

public class BazaarInteractable : Interactable
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public override void OnInteractPress()
    {
        base.OnInteractPress();

        // set our interactable
        BazaarCanvas.Instance.interactable = GetComponent<Interactable>();

        if (BazaarCanvas.Instance.isCanvasOpen)
        {
            BazaarCanvas.Instance.HideCanvas();
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
        }
        else
        {
            BazaarCanvas.Instance.ShowCanvas();
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
