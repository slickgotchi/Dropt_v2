using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GotchiHub;

public class GotchiSelectPortal : Interactable
{
    public override void OnPressOpenInteraction()
    {
        ThirdwebCanvas.Instance.ShowCanvas();
        GotchiSelectCanvas.Instance.ShowCanvas();
        PlayerInputMapSwitcher.Instance.SwitchToInUI();
    }

    //public override void OnPressCloseInteraction()
    //{
    //    Debug.Log("GotchiSelectPortal.OnPressCloseInteraction()");
    //    ThirdwebCanvas.Instance.HideCanvas();
    //    GotchiSelectCanvas.Instance.HideCanvas();
    //    PlayerInputMapSwitcher.Instance.SwitchToInGame();
    //}
}
