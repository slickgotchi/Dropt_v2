using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GotchiHub;

public class GotchiSelectPortal : Interactable
{
    public override void OnPressOpenInteraction()
    {
        ThirdwebCanvas.Instance.Container.SetActive(true);
        GotchiSelectCanvas.Instance.SetVisible(true);
        SetPlayerInputEnabled(false);
    }

    public override void OnPressCloseInteraction()
    {
        ThirdwebCanvas.Instance.Container.SetActive(false);
        GotchiSelectCanvas.Instance.SetVisible(false);
        SetPlayerInputEnabled(true);
    }
}
