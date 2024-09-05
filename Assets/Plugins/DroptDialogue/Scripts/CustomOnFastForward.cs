using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem.Wrappers;

public class CustomOnFastForward : MonoBehaviour
{
    public DroptUIMenuArrowController MenuPanel;

    private StandardUIContinueButtonFastForward uiContinueButtonFF;

    private void Awake()
    {
        uiContinueButtonFF = GetComponent<StandardUIContinueButtonFastForward>();
    }

    public void OnFastForward()
    {
        if (!MenuPanel.gameObject.activeSelf)
        {
            uiContinueButtonFF.OnFastForward();
        } else
        {
            MenuPanel.SelectCurrentResponse();
        }
    }
}
