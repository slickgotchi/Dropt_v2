using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem.Wrappers;

public class InteractableBarkNPC : InteractableNPC
{
    public override void OnPressOpenInteraction()
    {
        base.OnPressOpenInteraction();

        GetComponent<DialogueSystemTrigger>().Fire(transform);
    }

    public override void OnPressCloseInteraction()
    {
        base.OnPressCloseInteraction();

        GetComponentInChildren<StandardBarkUI>().Hide();
    }

    public override void OnTriggerFinishInteraction()
    {
        base.OnTriggerFinishInteraction();

        GetComponentInChildren<StandardBarkUI>().Hide();
    }
}
