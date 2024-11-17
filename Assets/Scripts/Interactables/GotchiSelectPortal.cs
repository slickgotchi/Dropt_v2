using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GotchiHub;

public class GotchiSelectPortal : Interactable
{
    public override void OnInteractPress()
    {
        ThirdwebCanvas.Instance.ShowCanvas();
        GotchiSelectCanvas.Instance.ShowCanvas();
    }
}
