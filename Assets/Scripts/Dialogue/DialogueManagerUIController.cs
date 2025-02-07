using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DialogueManagerUIController : MonoBehaviour
{
    private GameObject DialoguePanel;
    bool isDialogueActive = false;

    private void Update()
    {
        if (DialoguePanel == null)
        {
            var dpt = GetComponentInChildren<DialoguePanelTag>();
            if (dpt != null)
            {
                DialoguePanel = dpt.gameObject;
            }
            else
            {
                return;
            }
        }

        if (isDialogueActive == DialoguePanel.activeSelf) return;
        isDialogueActive = DialoguePanel.activeSelf;    

        if (DialoguePanel.activeSelf)
        {
            HideOtherUI();
            FreezePlayerMovement();
        } else
        {
            ShowOtherUI();
            UnfreezePlayerMovement();
        }
    }

    public void HideOtherUI()
    {
        var playerHudCanvas = PlayerHUDCanvas.Instance;
        if (playerHudCanvas != null)
        {
            playerHudCanvas.Hide();
        }
    }

    public void ShowOtherUI()
    {
        var playerHudCanvas = PlayerHUDCanvas.Instance;
        if (playerHudCanvas != null)
        {
            playerHudCanvas.Show();
        }

    }

    public void FreezePlayerMovement()
    {
        var playerControllers = Game.Instance.playerControllers;
        foreach (var playerController in playerControllers)
        {
            if (playerController.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                playerController.GetComponent<PlayerPrediction>().IsInputEnabled = false;
                Debug.Log("InputDisabled");
            }
        }
    }

    public void UnfreezePlayerMovement()
    {
        var playerControllers = Game.Instance.playerControllers;
        foreach (var playerController in playerControllers)
        {
            if (playerController.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                playerController.GetComponent<PlayerPrediction>().IsInputEnabled = true;
                Debug.Log("InputEnabled");
            }
        }
    }
}
