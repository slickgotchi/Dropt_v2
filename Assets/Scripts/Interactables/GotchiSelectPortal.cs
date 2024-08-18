using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GotchiHub;

public class GotchiSelectPortal : Interactable
{
    public override void OnStartInteraction()
    {
        ThirdwebCanvas.Instance.Container.SetActive(true);
        GotchiSelectCanvas.Instance.SetVisible(true);
    }

    public override void OnUpdateInteraction()
    {

    }

    public override void OnFinishInteraction()
    {
        ThirdwebCanvas.Instance.Container.SetActive(false);
        GotchiSelectCanvas.Instance.SetVisible(false);
    }

}
