using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GotchiHub;

public class GotchiSelectPortal : Interactable
{
    public override void OnStartInteraction()
    {
        ThirdwebCanvas.Instance.Container.SetActive(true);
        GotchiSelectCanvas.Instance.Container.SetActive(true);
        GotchiSelectCanvas.Instance.OpenGotchiSelectMenu();
    }

    public override void OnUpdateInteraction()
    {
        GotchiSelectCanvas.Instance.OpenGotchiSelectMenu();

    }

    public override void OnFinishInteraction()
    {
        ThirdwebCanvas.Instance.Container.SetActive(false);
        GotchiSelectCanvas.Instance.Container.SetActive(false);
    }

}
