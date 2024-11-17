using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DialogueManagerUIController : MonoBehaviour
{
    public GameObject DialoguePanel;
    bool isDialogueActive = false;

    private void Update()
    {
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
        var players = FindObjectsByType<PlayerPrediction>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                player.IsInputEnabled = false;
            }
        }
    }

    public void UnfreezePlayerMovement()
    {
        var players = FindObjectsByType<PlayerPrediction>(FindObjectsSortMode.None);
        foreach (var player in players)
        {
            if (player.GetComponent<NetworkObject>().IsLocalPlayer)
            {
                player.IsInputEnabled = true;
            }
        }
    }
}
