using UnityEngine;

public class LeaderboardInteractable : Interactable
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public override void OnInteractPress()
    {
        base.OnInteractPress();

        // set our interactable
        LeaderboardCanvas.Instance.interactable = GetComponent<Interactable>();

        if (LeaderboardCanvas.Instance.isCanvasOpen)
        {
            LeaderboardCanvas.Instance.HideCanvas();
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
        }
        else
        {
            LeaderboardCanvas.Instance.ShowCanvas();
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
