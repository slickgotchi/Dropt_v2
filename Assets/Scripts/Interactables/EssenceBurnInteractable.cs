using UnityEngine;

public class EssenceBurnInteractable : Interactable
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public override void OnInteractPress()
    {
        base.OnInteractPress();

        // set our interactable
        EssenceBurnCanvas.Instance.interactable = GetComponent<Interactable>();

        var selectedGotchiId = GotchiHub.GotchiDataManager.Instance.GetSelectedGotchiId();
        var isOffchainGotchi =
            GotchiHub.GotchiDataManager.Instance.GetOffchainGotchiDataById(selectedGotchiId) != null;

        if (isOffchainGotchi) return;

        if (EssenceBurnCanvas.Instance.isCanvasOpen)
        {
            EssenceBurnCanvas.Instance.HideCanvas();
            PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(interactionText, interactableType);
        }
        else
        {
            EssenceBurnCanvas.Instance.ShowCanvas();
            PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
        }
    }

    public override void OnTriggerEnter2DInteraction()
    {
        base.OnTriggerEnter2DInteraction();

        var selectedGotchiId = GotchiHub.GotchiDataManager.Instance.GetSelectedGotchiId();
        var isOffchainGotchi =
            GotchiHub.GotchiDataManager.Instance.GetOffchainGotchiDataById(selectedGotchiId) != null;

        var displayText = isOffchainGotchi ? "Can not burn free-play gotchi essense" : interactionText;
        PlayerHUDCanvas.Instance.ShowPlayerInteractionCanvii(displayText, interactableType);
    }

    public override void OnTriggerExit2DInteraction()
    {
        base.OnTriggerExit2DInteraction();

        PlayerHUDCanvas.Instance.HidePlayerInteractionCanvii(interactableType);
    }
}
